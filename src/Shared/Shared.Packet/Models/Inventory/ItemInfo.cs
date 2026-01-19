using Shared.Packet;
using System;
using System.Net.Sockets;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class ItemInfo
    {
        public RewardPaymentType ItemType { get; set; }
        public long Index { get; set; }
        public BigInteger ItemQty { get; set; }

        public ItemInfo() { }
        public ItemInfo(RewardPaymentType type, long id, BigInteger qty)
        {
            ItemType = type;
            Index = id;
            ItemQty = qty;
        }
    }
}
