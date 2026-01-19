using System;
using System.Diagnostics.CodeAnalysis;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Utility;

namespace Shared.Session.Data
{
    public class NtfReply : IReply
    {
        private readonly NtfPacketItem _packet;

        public NtfReply(Type type)
            : this(type, null)
        {
        }

        public NtfReply([NotNull] NtfBase data)
            : this(data.GetType(), data)
        {
        }

        private NtfReply(Type type, NtfBase data)
        {
            var header = PacketHeaderTable.GetHeaderData(type);
            _packet = new NtfPacketItem(header, data);
        }

        public PacketHeaderData GetHeaderData()
        {
            return _packet.GetHeaderData();
        }

        public IPacketItem MakePacketItem()
        {
            return _packet;
        }

        public IPacketItem MakePacketItem(ushort requestId)
        {
            throw new NotImplementedException();
        }
    }
}