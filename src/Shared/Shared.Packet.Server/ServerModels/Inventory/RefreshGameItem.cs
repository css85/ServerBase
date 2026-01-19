using Shared.Entities.Models;
using Shared.Packet.Models;
using System;
using System.Collections.Generic;

namespace Shared.ServerModel
{
    public class RefreshGameItem
    {   
        public List<ItemInfo> RefreshItems { get; set; } = new();  
        public VipBase VipBase { get; set; } = new();   // vip level 이 -1 일 경우 갱신하지 않는다 
    }
}
