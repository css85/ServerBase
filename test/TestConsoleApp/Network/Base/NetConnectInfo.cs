using Shared;

namespace TestConsoleApp.Network.Base
{
    public class NetConnectInfo
    {
        public NetServiceType[] ServiceTypes;
        public ProtocolType ProtocolType;
        public string Host;
        public int Port;

        public static NetConnectInfo NullConnectInfo = new NetConnectInfo
        {
            ServiceTypes = new NetServiceType[0],
            ProtocolType = ProtocolType.None,
            Host = "",
            Port = 0,
        };
    }
}
