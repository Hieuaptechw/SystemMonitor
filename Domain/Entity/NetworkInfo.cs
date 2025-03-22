using System;
using System.Collections.Generic;

namespace Domain.Entity
{
    public class NetworkInfo
    {
        public string IPAddress { get; set; } = string.Empty;
        public List<int> OpenPorts { get; set; } = new List<int>();
        public Dictionary<DatabaseType, List<string>> Databases { get; set; } = new Dictionary<DatabaseType, List<string>>();
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    public enum DatabaseType
    {
        SqlServer,
        MySql,
        MongoDb,
    }
}
