using System.Threading.Tasks;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Session.Data;

namespace TestConsoleApp.Network
{
    public interface INetworkConnection
    {
        Task ConnectAsync(string host, int port);
        void Close();

        bool IsConnected();
        Task SendSimpleAsync(RequestReply reply);
        Task<IPacketItem> SendReceiveAsync(RequestReply reply);
        string SecretKey { get; }
        void SetSecretKey(string secret);
        IConnection GetConnection();
    }
}
