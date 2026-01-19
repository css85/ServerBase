using System;

namespace Shared.Packet.Extension
{
    public static class PacketItemExtensions
    {
        public static PacketType Type(this IPacketItem packetItem)
        {
            return packetItem.GetHeaderData()?.PacketType ?? PacketType.None;
        }

        public static PacketHeader Header(this IPacketItem packetItem)
        {
            return packetItem.GetHeaderData()?.Header ?? PacketHeader.None;
        }

        public static Type DataType(this IPacketItem packetItem)
        {
            return packetItem.GetHeaderData()?.DataType;
        }
    }
}