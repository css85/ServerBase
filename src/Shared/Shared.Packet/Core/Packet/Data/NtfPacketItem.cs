using Shared.Model;
using Shared.Packet;
using Shared.Packet.Utility;

namespace Shared.Packet.Data
{
    public class NtfPacketItem : PacketItemBase
    {
        public NtfPacketItem(PacketHeaderData header, IPacketData data) : base(header, data)
        {
        }

        public NtfPacketItem(PacketHeaderData header, byte[] dataBytes) : base(header, null)
        {
            _dataBytes = dataBytes;
        }

        public NtfPacketItem(IPacketItem packetItem, IPacketData data) : base(packetItem.GetHeaderData(), data)
        {
        }

        public override int GetHeaderSize()
        {
            return Const.NTF_HEADER_SIZE;
        }

        public override ushort GetRequestId()
        {
            return 0;
        }

        public override int GetResult()
        {
            return 0;
        }
    }
}