namespace Domain.Entity
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public int PID { get; set; } 
        public double CPUUsage { get; set; } 
        public double RAMUsage { get; set; }
        public DateTime StartedAt { get; set; }
    }
}
