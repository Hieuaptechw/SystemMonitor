using System.Collections.Generic;

namespace Domain.Entity
{
    public class NetworkInfo
    {
        public int Id { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public List<string> OpenPorts { get; set; } = new List<string>();
        public List<string> Databases { get; set; } = new List<string>();
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
