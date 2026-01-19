
using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class PushOptionInfo
    {   
        public bool ServerPush { get; set; }
        public bool NightPush { get; set; }
    }
}
