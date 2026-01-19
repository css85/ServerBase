
using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class RefreshInventoryInfo
    {
        
        public List<ItemInfo> ItemList { get; set; } = new List<ItemInfo>();
        public List<ItemInfo> PointList { get; set; } = new List<ItemInfo>();
        public List<CurrencyInfo> CurrencyList { get; set; } = new List<CurrencyInfo>();
        public VipBase VipBase { get; set; } = new VipBase();   // vip level 이 -1 일 경우 갱신하지 않는다 
    }
}
