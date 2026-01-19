using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class InAppStoreInfo
    {
        public long Index { get; set; }
        public bool IsAvailableBonus { get; set; } // 첫구매 보너스 여부

        public InAppStoreInfo() { }

        public InAppStoreInfo(long index, bool isAvailable = true) 
        {
            Index = index;
            IsAvailableBonus = isAvailable; 
        }
    }
}
