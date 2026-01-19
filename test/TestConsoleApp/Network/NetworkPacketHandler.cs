using Shared.Model;
using Shared.Packet;
using TestConsoleApp.Network.Base;

namespace TestConsoleApp.Network
{
    public interface INetworkHandler
    {
        void OnConnected(NetConnectInfo connectInfo);
        void OnDisconnected(NetConnectInfo connectInfo);
        void OnReceived(NetConnectInfo connectInfo, IPacketItem packetItem,IPacketData packetData);
    }
}
