using Shared.Packet;

namespace Shared.Models
{
    public class AppMetricInfo
    {
        public int AppId { get; set; }
        public long CurrentConnections { get; set; }
        public long CurrentProcessingPackets { get; set; }
        
        public double ConnectionsPerSec { get; set; }
        public double ProcessingPacketsPerSec { get; set; }
  
    }
}