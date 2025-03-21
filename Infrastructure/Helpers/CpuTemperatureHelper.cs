using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class CpuTemperatureHelper
    {
        public static double GetCPUTemperature()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 0.0;

            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
                var temperature = searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["CurrentTemperature"];

                return temperature != null ? (Convert.ToDouble(temperature) - 2732) / 10.0 : 0.0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy nhiệt độ CPU: {ex.Message}");
                return 0.0;
            }
        }
    }
}
