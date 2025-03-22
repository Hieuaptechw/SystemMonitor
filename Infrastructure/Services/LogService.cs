using System;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class LogService
    {
        private readonly string _logFilePath;

        public LogService()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "API", "Logs");
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, "log.txt");
        }

        public async Task LogInfo(string message)
        {
            await WriteLogAsync("INFO", message);
        }

        public async Task LogError(string message)
        {
            await WriteLogAsync("ERROR", message);
        }

        private async Task WriteLogAsync(string logType, string message)
        {
            string logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}";

            try
            {
                await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi ghi log: {ex.Message}");
            }
        }
    }
}
