using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class UserConnectInfo
    {
        public long UserSeq { get; set; }
        public SessionLocation Session { get; set; }
        public long ConnectTime { get; set; }
    }
}
