using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                        // Kiểm tra nếu tiến trình có cửa sổ chính với tiêu đề không rỗng
                        if (!string.IsNullOrEmpty(process.MainWindowTitle))
                        {
                            var processInfo = new ProcessInfo
                            {
                                Id = process.Id,
                                ProcessName = process.ProcessName,
                                MainWindowTitle = process.MainWindowTitle,
                                CPUUsage = GetProcessCpuUsage(process),
                                RAMUsage = process.WorkingSet64 / (1024.0 * 1024.0),
                                StartedAt = process.StartTime
                            };

                            lock (applicationList)
                            {
                                applicationList.Add(processInfo);
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Xử lý khi tiến trình đã thoát
                        Console.WriteLine($"Không thể lấy thông tin từ tiến trình {process.ProcessName}: {ex.Message}");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Bỏ qua các tiến trình mà chúng ta không có quyền truy cập
                        Console.WriteLine($"Không thể truy cập tiến trình {process.ProcessName}: Quyền truy cập bị từ chối.");
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        // Bỏ qua các tiến trình gây ra lỗi Win32
                        Console.WriteLine($"Không thể truy cập tiến trình {process.ProcessName}: Lỗi hệ thống.");
                    }
                    catch (Exception ex)
                    {
                        // Xử lý các ngoại lệ khác
                        Console.WriteLine($"Không thể lấy thông tin từ tiến trình {process.ProcessName}: {ex.Message}");
                    }
                });
            });

            return applicationList.OrderBy(p => p.ProcessName).ToList();
        }

        private double GetProcessCpuUsage(Process process)
        {
            try
            {
                // Lấy thời gian sử dụng CPU ban đầu
                TimeSpan startCpuUsage = process.TotalProcessorTime;
                DateTime startTime = DateTime.UtcNow;

                // Đợi một khoảng thời gian để tính toán mức sử dụng CPU
                Task.Delay(500).Wait();

                // Lấy thời gian sử dụng CPU sau khoảng thời gian chờ
                TimeSpan endCpuUsage = process.TotalProcessorTime;
                DateTime endTime = DateTime.UtcNow;

                // Tính toán mức sử dụng CPU
                double cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                double totalMsPassed = (endTime - startTime).TotalMilliseconds;

                // Nhân với số lượng bộ xử lý logic
                double cpuUsageTotal = (cpuUsedMs / (totalMsPassed * Environment.ProcessorCount)) * 100;

                return Math.Round(cpuUsageTotal, 2);
            }
            catch
            {
                return 0; // Trả về 0 nếu không thể lấy thông tin CPU
            }
        }
    }
}
