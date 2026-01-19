using Shared.Packet;
using Shared.Packet.Utility;

namespace Shared.Session.Data
{
    public interface IReply
    {
        PacketHeaderData GetHeaderData();

        IPacketItem MakePacketItem();
        IPacketItem MakePacketItem(ushort requestId);
    }
}
