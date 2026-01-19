using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.TcpNetwork.Transport;
using TestConsoleApp.Network.Base;
using TestConsoleApp.User;

namespace TestConsoleApp.Network
{
    public class NetworkManager
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<NetworkManager> _logger;
        private readonly Dictionary<NetServiceType, NetConnectInfo> _connectInfoMap;
        private readonly ITcpSetting _tcpConnectionSettings;
        private readonly IServiceProvider _services;
        public NetworkManager(IServiceProvider services, ILoggerFactory loggerFactory, IEnumerable<NetConnectInfo> netConnectInfos)
        {
            _services = services;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<NetworkManager>();
            _connectInfoMap = netConnectInfos.SelectMany(p =>
            {
                return p.ServiceTypes.Select(serviceType => KeyValuePair.Create(serviceType, p));
            }).ToDictionary(p => p.Key, p => p.Value);
            _tcpConnectionSettings = _services.GetRequiredService<ITcpSetting>();
        }

        public NetConnectInfo GetConnectInfo(NetServiceType serviceType, string host = "", int port = 0)
        {
            var connectInfo = _connectInfoMap[serviceType];
            if (connectInfo == null)
                return null;

            if (host == "" && port == 0)
                return connectInfo;

            return new NetConnectInfo
            {
                ServiceTypes = connectInfo.ServiceTypes,
                ProtocolType = connectInfo.ProtocolType,
                Host = host == "" ? connectInfo.Host : host,
                Port = port == 0 ? connectInfo.Port : port,
            };
        }

        public async Task<INetworkConnection> ConnectAsync(TestUserContext testUserContext, NetServiceType serviceType, string host = "", int port = 0, string token = null)
        {
            var connectInfo = GetConnectInfo(serviceType, host, port);
            if (connectInfo == null)
            {
                throw new NotImplementedException("Null ConnectionInfo");
            }

            INetworkConnection connection = connectInfo.ProtocolType switch
            {
                ProtocolType.Tcp => new TcpNetworkConnection(_loggerFactory, _tcpConnectionSettings, connectInfo, testUserContext),
                ProtocolType.Http => new HttpConnection(_services, connectInfo, testUserContext, token),
                ProtocolType.WebSocket => throw new NotImplementedException(),
                ProtocolType.None => throw new InvalidOperationException("ProtocolType is None"),
                _ => throw new ArgumentOutOfRangeException()
            };

            await connection.ConnectAsync(host, port);

            return connection;
        }
    }
}
