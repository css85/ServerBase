using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class TableActiveInfo
    {
        public long TableIndex { get; set; }     
        public long ToDtTick { get; set; }
    }
}
