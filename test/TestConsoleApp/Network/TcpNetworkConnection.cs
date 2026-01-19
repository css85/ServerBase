using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Model;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Extension;
using Shared.Packet.Utility;
using Shared.PacketModel;
using Shared.Session.Data;
using Shared.TcpNetwork.Base;
using Shared.TcpNetwork.Transport;
using Shared.Utility;
using TestConsoleApp.Network.Base;
using TestConsoleApp.User;

namespace TestConsoleApp.Network
{
    public class TcpNetworkConnection : INetworkConnection, ITcpConnectionLogger
    {
        private readonly ILogger<TcpNetworkConnection> _logger;

        public readonly NetConnectInfo ConnectInfo;

        private readonly ITcpSetting _tcpConnectionSettings;
        private readonly INetworkHandler _networkHandler;

        private TcpConnector _tcpConnector;
        private TcpConnection _tcpConnection;

        private Dictionary<ushort, TaskCompletionSource<IPacketItem>> _responseWaitCompletionSourceMap = new();

        private long _userSeq;

        private ushort _currentRequestId;
        private readonly IPacketSerializer _packetSerializer;
        private readonly ITestContext _testContext;

        public TcpNetworkConnection(ILoggerFactory loggerFactory, ITcpSetting tcpConnectionSettings, NetConnectInfo connectInfo, TestUserContext testContext)
        {
            _logger = loggerFactory.CreateLogger<TcpNetworkConnection>();
            ConnectInfo = connectInfo;
            _tcpConnectionSettings = tcpConnectionSettings;
            _networkHandler = testContext;
            _testContext = testContext;
            
            _packetSerializer = tcpConnectionSettings.PacketSerializer;
        }

        public string SecretKey => _tcpConnection.IsAuthenticated?_tcpConnection.SecretKey??_testContext.GateSecretKey:_testContext.GateSecretKey;

        private ushort GetRequestId()
        {
            _currentRequestId++;
            if (_currentRequestId == 0)
                _currentRequestId++;

            return _currentRequestId;
        }

        public void SetUserSeq(long userSeq)
        {
            _userSeq = userSeq;
        }

        public void SetSecretKey(string secret)
        {
            _tcpConnection.SetSecretKey(secret);
        }

        public IConnection GetConnection()
        {
            return _tcpConnection;
        }

        public async Task ConnectAsync(string host, int port)
        {
            host = string.IsNullOrEmpty(host) ? ConnectInfo.Host : host;
            port = port == 0 ? ConnectInfo.Port : port;

            var endPoint = new IPEndPoint(IPAddress.Parse(host), port);

            _tcpConnector = new TcpConnector();
            _tcpConnector.Connected += OnConnected;
            _tcpConnector.ConnectFailed += OnConnectFailed;
            await _tcpConnector.ConnectAsync(endPoint, CancellationToken.None);
        }

        public void Close()
        {
            if (_tcpConnection == null) return;
            
            _tcpConnection.Close();
            _tcpConnection = null;
        }

        public bool IsConnected()
        {
            return _tcpConnection != null && _tcpConnection.IsOpen;
        }

        private void OnConnected(TcpConnector tcpConnection, Socket socket)
        {
            //유저별 엑세스 토큰
            _tcpConnection = new TcpConnection(this, _tcpConnectionSettings, socket, 0,_testContext.GateSecretKey);
            _tcpConnection.Received += OnReceived;
            _tcpConnection.Authenticated += OnAuthenticated;
            _tcpConnection.Open();

            _networkHandler.OnConnected(ConnectInfo);
        }

        private void OnAuthenticated(IConnection conntion)
        {
            conntion.IsAuthenticated= true;
        }

        private void OnConnectFailed(TcpConnector tcpConnector, SocketError socketError)
        {
            _logger.LogError("{UserSeq}| OnConnectFailed: {SocketError}", _userSeq, socketError);

            _networkHandler.OnDisconnected(ConnectInfo);
        }

        private void OnReceived(IConnection connection, object packet)
        {
            var packetItem = packet as IPacketItem;
            IPacketData packetData = null;
            if( packetItem.GetDataBytes()!=null && packetItem.GetDataBytes().Length>0)
            {
                var secretKey = packetItem.GetHeaderData().PacketType switch
                {
                    PacketType.Ntf => _testContext.GateSecretKey,
                    _ => SecretKey
                };
                var bytes = EncryptProvider.DecryptAes256(secretKey, packetItem.GetDataBytes());
                packetItem.SetDataBytes(bytes);
                
                packetData = JsonTextSerializer.Deserialize(packetItem.GetDataBytes(), packetItem.DataType()) as IPacketData;
                
                ChangeSecretKeyIfConnectSession(packetData);
            }
            
            _networkHandler.OnReceived(ConnectInfo, packetItem, packetData);
           
            if (packetItem.Type() == PacketType.Response)
            {
                if (_responseWaitCompletionSourceMap.TryGetValue(packetItem.GetRequestId(), out var completionSource))
                {
                    completionSource.SetResult(packetItem);

                    _responseWaitCompletionSourceMap.Remove(packetItem.GetRequestId());
                }
            }
            
        }

        private void ChangeSecretKeyIfConnectSession(IPacketData packetData)
        {
            switch (packetData)
            {
                case ConnectSessionRes connectRes:
                    OnAuthenticated(_tcpConnection);
                    _tcpConnection.SetSecretKey(connectRes.EncryptKey);
                    break;

                default: break;
            }
        }

        public Task SendSimpleAsync(RequestReply reply)
        {
            var sendPacketItem = reply.MakePacketItem(GetRequestId());

            if (IsConnected() == false)
                return Task.CompletedTask;

            try
            {
                _tcpConnection.Send(sendPacketItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send Error");
                Fail(sendPacketItem);
            }

            return Task.CompletedTask;
        }

        public async Task<IPacketItem> SendReceiveAsync(RequestReply reply)
        {
            var sendPacketItem = reply.MakePacketItem(GetRequestId());
            var dataBytes = _packetSerializer.Serialize(sendPacketItem);
            if(sendPacketItem.GetData()!=null && dataBytes.Length > 0)
            {
                var bytes = EncryptProvider.EncryptAes256(SecretKey, dataBytes);
                sendPacketItem.SetDataBytes(bytes);
            }
            
            if (IsConnected() == false)
            {
                return Fail(sendPacketItem);
            }

            var responseWaitCompletionSource = new TaskCompletionSource<IPacketItem>();
            _responseWaitCompletionSourceMap.Add(sendPacketItem.GetRequestId(), responseWaitCompletionSource);

            try
            {
                _tcpConnection.Send(sendPacketItem);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,"{Type}",sendPacketItem.GetType().Name);
                _responseWaitCompletionSourceMap.Remove(sendPacketItem.GetRequestId());

                return Fail(sendPacketItem);
            }
            var result = await responseWaitCompletionSource.Task;
            return result;
        }

        private IPacketItem Fail(IPacketItem packetItem)
        {
            var resType = PacketHeaderTable.GetResType(packetItem.Header());
            var resHeaderData = PacketHeaderTable.GetHeaderData(resType);
            var resPacket = new ResponsePacketItem(resHeaderData, (IPacketData) null, (int) ResultCode.Fail, 0);
            _networkHandler.OnReceived(ConnectInfo, resPacket,null);
            return resPacket;
        }
        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }

        public void LogTrace(string message, Exception e)
        {
            _logger.LogTrace(e, message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogWarning(string message, Exception e)
        {
            _logger.LogWarning(e, message);
        }
    }
}