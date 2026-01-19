using Shared.Model;
using System;

namespace Shared.Packet
{
    [Serializable]
    public class ResponsePacket<T>: ResponsePacketBase
        where T : ResponseBase
    {
        public ResponsePacket(byte major, byte minor):base (major,minor)
        {
        }
        
        public T Body;
    }
}
