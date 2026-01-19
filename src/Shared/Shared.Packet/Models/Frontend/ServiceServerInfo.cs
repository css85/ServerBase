using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class ServiceServerInfo
    {     
        public NetServiceType Type { get; set; } //서비스 타입..
        public string Url { get; set; }
    }
}
