namespace Shared.Models
{
    public class ServerMetric
    {
        public int AppId { get; set; }
        public int AppGroupId { get; set; }
        
        public double CpuUsage { get; set; }
        public double MemUsage { get; set; }

        public long ProcessingPacketCount { get; set; }
        
        public long ActiveSessionCount { get; set; }
    }
}