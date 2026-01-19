using System;
using System.Diagnostics.CodeAnalysis;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Utility;

namespace Shared.Session.Data
{
    public class ResponseReply : IReply
    {
        public readonly PacketHeaderData Header;
        public readonly ResponseBase Data;
        public readonly int Result;
        public readonly ushort RequestId;

        public ResponseReply(Type type, int result, ushort requestId)
            : this(type, null, result, requestId)
        {
        }
        public ResponseReply([NotNull] ResponseBase data, int result, ushort requestId)
            : this(data.GetType(), data, result, requestId)
        {
        }

        public ResponseReply(Type type, ResponseBase data, int result, ushort requestId)
        {
            Header = PacketHeaderTable.GetHeaderData(type);
            Data = data;
            Result = result;
            RequestId = requestId;
        }

        public PacketHeaderData GetHeaderData()
        {
            return Header;
        }

        public IPacketItem MakePacketItem()
        {
            return new ResponsePacketItem(Header, Data, Result, RequestId);
        }

        public IPacketItem MakePacketItem(ushort requestId)
        {
            return new ResponsePacketItem(Header, Data, Result, requestId);
        }
    }
}