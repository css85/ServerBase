using Shared.Model;
using Shared.Packet;
using Shared.Packet.Utility;

namespace Shared.Packet.Data
{
    public class ResponsePacketItem : PacketItemBase
    {
        private readonly int _result;
        private readonly ushort _requestId;

        public ResponsePacketItem(PacketHeaderData header, IPacketData data, int result, ushort requestId)
            : base(header, data)
        {
            _result = result;
            _requestId = requestId;
        }

        public ResponsePacketItem(PacketHeaderData header, byte[] stream, int result, ushort requestId) : base(header, null)
        {
            _result = result;
            _requestId = requestId;
            _dataBytes = stream;
        }

        public ResponsePacketItem(IPacketItem packetItem, IPacketData data) : base(packetItem.GetHeaderData(), data)
        {
            _result = packetItem.GetResult();
            _requestId = packetItem.GetRequestId();
        }

        public override int GetHeaderSize()
        {
            return Const.RES_HEADER_SIZE;
        }

        public override int GetResult()
        {
            return _result;
        }

        public override ushort GetRequestId()
        {
            return _requestId;
        }
    }
}