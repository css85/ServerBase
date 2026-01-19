using System;
using Shared.Packet.Models;

namespace Shared.Models
{
    [Serializable]
    public class UserSimpleInfo : UserSimple
    {
        public SessionLocation Session { get; set; }
        public long ConnectTime { get; set; }
    }
}
