using Shared.TcpNetwork.Base;

namespace Shared.TcpNetwork.Transport
{
    
    public class TcpConnectionSettingBase : ITcpSetting
    {

        public bool SocketNoDelay { get; set; }

        public int ReceiveBufferSize { get; set; }
        public int ReceiveBufferMaxSize { get; set; }
        public int SendBufferSize { get; set; }
        public int SendBufferMaxSize { get; set; }

        public IPacketSerializer PacketSerializer { get; set; }

        public TcpConnectionSettingBase()
        {
            SocketNoDelay = true;

            ReceiveBufferSize = 1024;
            ReceiveBufferMaxSize = 10485760;
            SendBufferSize = 1024;
            SendBufferMaxSize = 10485760;
        }
    }
}
