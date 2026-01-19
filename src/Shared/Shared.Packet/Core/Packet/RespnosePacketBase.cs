using System;

namespace Shared.Packet
{
    [Serializable]
    public class ResponsePacketBase
    {
        public ResponsePacketBase(byte major, byte minor)
        {
            Header = new PacketHeader(major, minor);
        }
        public int Result;
        public PacketHeader Header;
    }
}
