using System;

namespace Shared.Packet
{
    [Serializable]
    public class RequestPacketBase
    {
        public RequestPacketBase(byte major, byte minor)
        {
            Header = new PacketHeader(major, minor);
        }

        public PacketHeader Header;

        public Session Session;
    }
}
