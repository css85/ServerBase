using System;

namespace Shared.Packet.Extensions
{
    public static class PacketTypeExtension
    {
        public static int GetHeaderSize(this PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.Request:
                    return Const.REQ_HEADER_SIZE;
                case PacketType.Response:
                    return Const.RES_HEADER_SIZE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(packetType), packetType, null);
            }
        }
    }
}
