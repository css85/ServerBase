using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Model;
using Shared.Models;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.Packet.Models;
using Shared.Packet.Utility;
using Shared.PacketModel;
using TestConsoleApp.Network;
using TestConsoleApp.Network.Base;
using static Shared.Session.Extensions.ReplyExtensions;
using ResponseBase = Shared.Model.ResponseBase;

namespace TestConsoleApp.User
{
    public class DummyUserInfo
    {
        public DummyUserInfo()
        {
        
        }
        
        public string AccountId;
    }

    public interface ITestContext
    {
        public string GateSecretKey { get; }
    }
    
    public class TestUserContext : INetworkHandler , ITestContext
    {
        private readonly ILogger<TestUserContext> _logger;
        public string GateSecretKey { get; private set; }

        private readonly AppContext _app;

        private readonly ConcurrentDictionary<NetServiceType, INetworkConnection> _connectionMap = new();
        public long UserSeq { get; private set; } 
        public GetServerInfosRes GateInfo { get; private set; }
        public DummyUserInfo DummyUserInfo { get; private set; }

        private long _sendPacketCount;
        private long _receivePacketCount;
        public long SendCount => Interlocked.Read(ref _sendPacketCount);
        public long ReceiveCount => Interlocked.Read(ref _receivePacketCount);

        public Random Random { get; private set; }

        public long ServerTime { get; private set; }


        public object Data;

        public T GetData<T>()
        {
            return (T) Data;
        }

        public void SetData(object data)
        {
            Data = data;
        }

        public TestUserContext(AppContext appContext)
        {
            _app = appContext;
            DummyUserInfo = new DummyUserInfo();
            var loggerFactory = appContext.AppServices.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<TestUserContext>();
        }
        public void ClearLogs()
        {
            Interlocked.Exchange(ref _sendPacketCount, 0);
            Interlocked.Exchange(ref _receivePacketCount, 0);
        }

        public bool IsConnecting(NetServiceType serviceType)
        {
            return _connectionMap.TryGetValue(serviceType, out var connection) && connection.IsConnected();
        }

        public Task<bool> ConnectAsync(NetServiceType[] serviceTypes, string hostAndPort, string token)
        {
            var host = "";
            var port = 0;
            var splitIndex = hostAndPort.LastIndexOf(":", hostAndPort.Length - 1, StringComparison.Ordinal);
            if (splitIndex > 0)
            {
                host = new string(hostAndPort.AsSpan(0, splitIndex));
                var portStringSpan = hostAndPort.AsSpan(splitIndex + 1, hostAndPort.Length - splitIndex - 1);
                if (int.TryParse(portStringSpan, out port) == false)
                    port = 0;
            }

            return ConnectAsync(serviceTypes, host, port, token);
        }

        public async Task<bool> ConnectAsync(NetServiceType[] serviceTypes, string host, int port, string token)
        {
            foreach (var serviceType in serviceTypes)
            {
                if (_connectionMap.ContainsKey(serviceType))
                    return false;
            }

            var connection = await _app.Net.ConnectAsync(this, serviceTypes[0], host, port, token);

            foreach (var serviceType in serviceTypes)
            {
                _connectionMap.TryAdd(serviceType, connection);
            }

            return true;
        }

        private void SetHttpToken(NetServiceType serviceType, string token)
        {
            if (_connectionMap.TryGetValue(serviceType, out var connection))
            {
                if (connection is HttpConnection httpConnection)
                {
                    httpConnection.SetToken(token);
                }
            }
        }
        
        public void CloseConnection(NetServiceType netServiceType)
        {
            if (_connectionMap.TryRemove(netServiceType, out var connection))
            {
                connection.Close();
            }
        }

        public void CloseAllConnections()
        {
            foreach (var connection in _connectionMap)
            {
                connection.Value.Close();
            }
            _connectionMap.Clear();
        }

        private async Task SendSimpleAsync(RequestBase request, NetServiceType serviceType = NetServiceType.None)
        {
            var header = PacketHeaderTable.GetHeaderData(request.GetType());
            if (serviceType == NetServiceType.None)
                serviceType = header.ServiceType.NetServiceType;

            if (_connectionMap.TryGetValue(serviceType, out var connection) == false)
            {
                var resType = PacketHeaderTable.GetResType(header.Header);
                // var resHeaderData = PacketHeaderTable.GetHeaderData(resType);
                // var resPacket = new ResponsePacketItem(resHeaderData, (IPacketData)null, (int) ResultCode.NotConnected, 0);
                // OnReceived(NetConnectInfo.NullConnectInfo, resPacket);
                LogWarning(ResultCode.NotConnected, resType, null, 0);
                return;
            }

            await connection.SendSimpleAsync(MakeReqReply(request));
            Interlocked.Increment(ref _sendPacketCount);
        }

        public async Task<ResponseResult<TResponse>> SendAsync<TResponse>(RequestBase request,
            NetServiceType serviceType = NetServiceType.None) where TResponse : ResponseBase
        {
            var header = PacketHeaderTable.GetHeaderData(request.GetType());
            if (serviceType == NetServiceType.None)
                serviceType = header.ServiceType.NetServiceType;

            if (_connectionMap.TryGetValue(serviceType, out var connection) == false)
            {
                var resType = PacketHeaderTable.GetResType(header.Header);
                // var resHeaderData = PacketHeaderTable.GetHeaderData(resType);
                // var resPacket = new ResponsePacketItem(resHeaderData, (IPacketData)null, (int) ResultCode.NotConnected, 0);
                // OnReceived(NetConnectInfo.NullConnectInfo, resPacket);
                LogWarning(ResultCode.NotConnected, resType, null, 0);
                return null;
            }
            
            var responsePacket = await connection.SendReceiveAsync(MakeReqReply(request));
            
            Interlocked.Increment(ref _sendPacketCount);

            var response = responsePacket.GetData();
            var returnData = new ResponseResult<TResponse>
            {
                Response = (TResponse) response,
                Result = responsePacket.GetResult()
            };

            return returnData;
        }

        private void LogWarning(ResultCode retcode,Type type, IPacketData data,int requestId)
        {
            _logger.LogWarning("{Type} ,{ResultCode} | {UserSeq}({RequestId}) | {Data} ",type.Name,retcode,UserSeq,requestId,data);
        }

        private void SetResponse(IPacketData response, IPacketItem packet)
        {
            // result 값이 0이 아닌 경우에도 파싱을 하는 데이터가 있다면 아래에서 처리하도록 한다
            if (packet.GetResult() != 0)
            {
                _logger.LogInformation($"Worng Response:{response}, {packet.GetResult()}");
                return;
            }

            switch (response)
            {

                default:
                
                case ConnectSessionRes:
                    break;
            }

            if (response is GetServerInfosRes getServerInfosRes)
            {
                GateInfo = getServerInfosRes;
            }
        }
         

       
        public void OnConnected(NetConnectInfo connectInfo)
        {
            // _logger.LogInformation("OnConnected");
        }

        public void OnDisconnected(NetConnectInfo connectInfo)
        {
            // _logger.LogInformation("OnDisconnected");
        }

        public void OnReceived(NetConnectInfo connectInfo, IPacketItem packetItem , IPacketData packetData)
        {
            if (packetItem.GetResult() != (int)ResultCode.Success)
            {
                LogWarning(packetItem.GetResultCode(),packetItem.DataType(),packetItem.GetData(),packetItem.GetRequestId());
                return;
            }

            var data = packetData;
            if (data == null)
                data = (IPacketData) Activator.CreateInstance(packetItem.DataType());

            if (packetItem.Header().Major == (byte)MAJOR.Common &&
                packetItem.Header().Minor == (byte)COMMON_MINOR.NTFPingCheck)
            {
                var pingNtf = (PingCheckNtf) data;
                SendSimpleAsync(new PingReq(), (NetServiceType) pingNtf.NetServiceType).Wait();
            }
            else
            {
                Interlocked.Increment(ref _receivePacketCount);
            }
            
            SetResponse(data, packetItem);
        }

    }
    public class ResponseResult<TResponse> where TResponse : ResponseBase
    {
        public TResponse Response;
        public int Result;
    }
}