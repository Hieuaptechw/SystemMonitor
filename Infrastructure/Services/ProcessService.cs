using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entity;

namespace Infrastructure.Services
{
    public class ProcessService : IProcessService
    {
        public async Task<List<ProcessInfo>> GetRunningApplicationsAsync()
        {
            var applicationList = new List<ProcessInfo>();
            var processes = Process.GetProcesses();

            await Task.Run(() =>
            {
                Parallel.ForEach(processes, process =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(process.MainWindowTitle))
                        {
                            var processInfo = new ProcessInfo
                            {
                                Id = process.Id,
                                ProcessName = process.ProcessName,
                                MainWindowTitle = process.MainWindowTitle,
                                CPUUsage = GetProcessCpuUsage(process),
                                RAMUsage = process.WorkingSet64 / (1024.0 * 1024.0),
                                StartedAt = process.StartTime,
                                NetworkSpeed = GetProcessNetworkSpeed(process.Id) // Lấy tốc độ mạng
                            };

                            lock (applicationList)
                            {
                                applicationList.Add(processInfo);
                            }
                        }
                    }
                    catch { }
                });
            });

            return applicationList.OrderBy(p => p.ProcessName).ToList();
        }

        private double GetProcessCpuUsage(Process process)
        {
            try
            {
                TimeSpan startCpuUsage = process.TotalProcessorTime;
                DateTime startTime = DateTime.UtcNow;

                Task.Delay(500).Wait();

                TimeSpan endCpuUsage = process.TotalProcessorTime;
                DateTime endTime = DateTime.UtcNow;

                double cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                double totalMsPassed = (endTime - startTime).TotalMilliseconds;

                double cpuUsageTotal = (cpuUsedMs / (totalMsPassed * Environment.ProcessorCount)) * 100;

                return Math.Round(cpuUsageTotal, 2);
            }
            catch
            {
                return 0;
            }
        }

        private double GetProcessNetworkSpeed(int processId)
        {
            try
            {
                var initialStats = GetProcessNetworkBytes(processId);
                Task.Delay(1000).Wait();
                var finalStats = GetProcessNetworkBytes(processId);

                double sentSpeed = (finalStats.SentBytes - initialStats.SentBytes) / (1024.0 * 1024.0); // Mbps
                double receivedSpeed = (finalStats.ReceivedBytes - initialStats.ReceivedBytes) / (1024.0 * 1024.0); // Mbps

                return Math.Round(sentSpeed + receivedSpeed, 2);
            }
            catch
            {
                return 0;
            }
        }

        private (long SentBytes, long ReceivedBytes) GetProcessNetworkBytes(int processId)
        {
            int bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, 2, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

            IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);
            try
            {
                if (GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, 2, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
                {
                    int rowSize = Marshal.SizeOf(typeof(MibTcpRowOwnerPid));
                    int tableSize = Marshal.ReadInt32(tcpTablePtr);
                    IntPtr rowPtr = tcpTablePtr + 4;

                    long sentBytes = 0;
                    long receivedBytes = 0;

                    for (int i = 0; i < tableSize; i++)
                    {
                        var row = Marshal.PtrToStructure<MibTcpRowOwnerPid>(rowPtr);
                        if (row.OwningPid == processId)
                        {
                            sentBytes += (long)row.OutOctets;
                            receivedBytes += (long)row.InOctets;

                        }
                        rowPtr += rowSize;
                    }

                    return (sentBytes, receivedBytes);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }

            return (0, 0);
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder, int ulAf, TcpTableClass tableClass, int reserved);

        private enum TcpTableClass
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcpRowOwnerPid
        {
            public uint State;
            public uint LocalAddr;
            public uint LocalPort;
            public uint RemoteAddr;
            public uint RemotePort;
            public uint OwningPid;
            public ulong InOctets;
            public ulong OutOctets;
        }
    }
}
