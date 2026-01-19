using Shared.Model;
using Shared.Packet.Utility;

namespace Shared.Packet.Data
{
    public class RequestPacketItem : PacketItemBase
    {
        private readonly PacketHeaderData _header;
        private readonly ushort _requestId;
        private readonly IPacketData _data;

        public RequestPacketItem(PacketHeaderData header, IPacketData data, ushort requestId) : base(header, data)
        {
            _header = header;
            _requestId = requestId;
            _data = data;
        }

        public RequestPacketItem(PacketHeaderData header, byte[] stream, ushort requestId) : base(header, null)
        {
            _header = header;
            _requestId = requestId;
            _dataBytes = stream;
        }

        public RequestPacketItem(IPacketItem packetItem, IPacketData packetData) : base(packetItem.GetHeaderData(), packetData)
        {
            _header = packetItem.GetHeaderData();
            _requestId = packetItem.GetRequestId();
            _data = packetData;
        }

        public override int GetHeaderSize()
        {
            return Const.REQ_HEADER_SIZE;
        }

        public override ushort GetRequestId()
        {
            return _requestId;
        }

        public override int GetResult()
        {
            return 0;
        }
    }
}