using System;
using System.Collections.Generic;

namespace Domain.Entity
{
    public class HardwareInfo
    {
        public int Id { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public string CPUName { get; set; } = string.Empty;
        public int CPUCores { get; set; }
        public int CPULogicalProcessors { get; set; }
        public double CPUSpeed { get; set; }
        public double CPUTemperature { get; set; }
        public List<GPUInfo> GPUs { get; set; } = new List<GPUInfo>();
        public int RAMTotal { get; set; }
        public int RAMUsed { get; set; }
        public List<DiskInfo> Disks { get; set; } = new List<DiskInfo>();
    }

    public class GPUInfo
    {
        public string Name { get; set; } = string.Empty;
        public double Speed { get; set; }
        public double Temperature { get; set; }
        public int Memory { get; set; }
    }

    public class DiskInfo
    {
        public string Name { get; set; } = string.Empty;
        public int TotalSize { get; set; }
        public int UsedSize { get; set; }
    }
}
