using Shared.Model;
using System;

namespace Shared.Packet
{
    [Serializable]
    public class RequestPacket<T> : RequestPacketBase 
        where T : RequestBase
    {
        public RequestPacket(byte major, byte minor) : base(major, minor) { }

        public T Body;
    }
}
