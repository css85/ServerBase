using Shared.TcpNetwork.Base;

namespace Shared.TcpNetwork.Transport
{
    public interface ITcpSetting
    {
        public bool SocketNoDelay { get; set; }

        public int ReceiveBufferSize { get; set; }
        public int ReceiveBufferMaxSize { get; set; }
        public int SendBufferSize { get; set; }
        public int SendBufferMaxSize { get; set; }

        public IPacketSerializer PacketSerializer { get; set; }
    }
}