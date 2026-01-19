using System;
using System.Diagnostics.CodeAnalysis;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Utility;

namespace Shared.Session.Data
{
    public class RequestReply : IReply
    {
        public readonly PacketHeaderData Header;
        public readonly RequestBase Data;

        public RequestReply(Type type) : this(type, null)
        {
        }
        public RequestReply([NotNull] RequestBase data) : this(data.GetType(), data)
        {
        }

        public RequestReply(Type type, RequestBase data)
        {
            Header = PacketHeaderTable.GetHeaderData(type);
            Data = data;
        }

        public PacketHeaderData GetHeaderData()
        {
            return Header;
        }

        public IPacketItem MakePacketItem()
        {
            return new RequestPacketItem(Header, Data, 0);
        }

        public IPacketItem MakePacketItem(ushort requestId)
        {
            return new RequestPacketItem(Header, Data, requestId);
        }
    }
}