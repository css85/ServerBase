using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class GachaStoreInfo
    {
        public long Index { get; set; }
        public long ToDtTick { get; set; }

        public GachaStoreInfo() { }

        public GachaStoreInfo(long gachaIndex, long tick )
        {
            this.Index = gachaIndex;   
            this.ToDtTick = tick;   
        }
    }
}
