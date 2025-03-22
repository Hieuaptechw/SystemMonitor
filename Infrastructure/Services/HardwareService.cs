using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HardwareService : IHardwareService
    {
        public async Task<HardwareInfo> GetHardwareInfoAsync()
        {
            return await Task.Run(() => new HardwareInfo
            {
                DeviceName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                RecordedAt = DateTime.UtcNow,
                CPUUsed = GetCPUUsed(),
                GPUs = GetGPUInfo(),
                RAMTotal = GetTotalRAM(),
                RAMUsed = GetUsedRAM(),
                Disks = GetDiskInfo()
            });
        }
        private static int GetCPUUsed()
        {
            try
            {
        
                  if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return 0;
                    var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                _ = cpuCounter.NextValue();
                Thread.Sleep(1000);
                return (int)cpuCounter.NextValue();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting CPU Usage: " + ex.Message);
                return -1; 
            }
        }


        private List<GPUInfo> GetGPUInfo()
        {
            var gpus = new List<GPUInfo>();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return gpus;
            using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                gpus.Add(new GPUInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown",
                    Memory = (int)((obj["AdapterRAM"] as uint? ?? 0) / (1024 * 1024))
                });
            }
            return gpus;
        }

        private int GetTotalRAM()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return 0;
            using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            return (int)((searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["TotalVisibleMemorySize"] as ulong? ?? 0) / 1024);
        }

        private int GetUsedRAM()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return 0;
            using var searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
            var freeMemory = (int)((searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["FreePhysicalMemory"] as ulong? ?? 0) / 1024);
            return GetTotalRAM() - freeMemory;
        }

        private List<DiskInfo> GetDiskInfo()
        {
            var disks = new List<DiskInfo>();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return disks;
            using var searcher = new ManagementObjectSearcher("SELECT Name, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                disks.Add(new DiskInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown",
                    TotalSize = (int)((obj["Size"] as ulong? ?? 0) / (1024 * 1024 * 1024)),
                    UsedSize = (int)(((obj["Size"] as ulong? ?? 0) - (obj["FreeSpace"] as ulong? ?? 0)) / (1024 * 1024 * 1024))
                });
            }
            return disks;
        }
    }
}
