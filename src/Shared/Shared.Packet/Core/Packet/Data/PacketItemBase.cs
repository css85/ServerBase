using System;
using Shared.Model;
using Shared.Packet.Utility;

namespace Shared.Packet.Data
{
    public abstract class PacketItemBase : IPacketItem
    {
        private readonly PacketHeaderData _header;
        private readonly IPacketData _data;
        protected byte[] _dataBytes;
        protected PacketItemBase(PacketHeaderData header, IPacketData data)
        {
            _header = header;
            _data = data;
        }

        public PacketHeaderData GetHeaderData()
        {
            return _header;
        }

        public abstract int GetHeaderSize();

        public abstract ushort GetRequestId();

        public abstract int GetResult();

        public ResultCode GetResultCode()
        {
            return (ResultCode) GetResult();
        }

        public IPacketData GetData()
        {
            return _data;
        }

        public void SetDataBytes(byte[] dataBytes)
        {
            _dataBytes = dataBytes;
        }

        public byte[] GetDataBytes()
        {
            return _dataBytes;
        }
    }
}