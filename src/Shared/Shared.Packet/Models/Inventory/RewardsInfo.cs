using Shared.Packet;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class RewardsInfo
    {
        public List<ItemInfo> Rewards { get; set; } = new List<ItemInfo>();
        public List<DuplicateItemInfo> DuplicateItemInfos { get; set; } = new List<DuplicateItemInfo>();
    }
}
