using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class ContentConfig
    {        
        public string ContentKey { get; set; }
        public bool IsActive { get; set; }
    }
}
