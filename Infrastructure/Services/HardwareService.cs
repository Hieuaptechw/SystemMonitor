using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Helpers;
using System;
using System.Collections.Generic;
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
                CPUName = GetCPUName(),
                CPUCores = Environment.ProcessorCount,
                CPULogicalProcessors = GetCPULogicalProcessors(),
                CPUSpeed = GetCPUSpeed(),
                CPUTemperature = GetCpuTemperature(),
                GPUs = GetGPUInfo(),
                RAMTotal = GetTotalRAM(),
                RAMUsed = GetUsedRAM(),
                Disks = GetDiskInfo()
            });
        }

        private string GetCPUName()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "Unknown";
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            return searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["Name"]?.ToString() ?? "Unknown";
        }

        private int GetCPULogicalProcessors()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return 0;
            using var searcher = new ManagementObjectSearcher("SELECT NumberOfLogicalProcessors FROM Win32_Processor");
            return Convert.ToInt32(searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["NumberOfLogicalProcessors"] ?? 0);
        }

        private double GetCPUSpeed()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return 0;
            using var searcher = new ManagementObjectSearcher("SELECT MaxClockSpeed FROM Win32_Processor");
            return Convert.ToDouble(searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["MaxClockSpeed"] ?? 0) / 1000.0;
        }

        public double GetCpuTemperature()
        {
            return CpuTemperatureHelper.GetCPUTemperature();
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
