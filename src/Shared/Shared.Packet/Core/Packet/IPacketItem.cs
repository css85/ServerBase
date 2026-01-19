using Shared.Model;
using Shared.Packet.Utility;

namespace Shared.Packet
{
    public interface IPacketItem
    {
        PacketHeaderData GetHeaderData();
        int GetHeaderSize();
        ushort GetRequestId();
        int GetResult();
        ResultCode GetResultCode();
        IPacketData GetData();
        
        void SetDataBytes(byte[] dataBytes);
        byte[] GetDataBytes();
    }
}