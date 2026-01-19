using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class PackageStoreInfo
    {
        public long Index { get; set; }
        public CycleType LimitCountCycle { get; set; }
        public int BuyCount { get; set; }
        public int Limit { get; set; }

        public PackageStoreInfo() { }

     
    }
}
