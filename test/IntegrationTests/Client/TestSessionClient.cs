using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Integration.Tests.Base;
using Shared;
using Shared.Packet;
using Shared.Network.Connection;
using Shared.Packet.Extension;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Utility;
using Shared.PacketModel;

namespace Integration.Tests.Client
{
    public class TestSessionClient : ITestClient
    {
        private readonly SessionServiceBase _sessionService;

        private readonly CancellationTokenSource _cts;

        private readonly LinkedList<IPacketItem> _receiveNtfList = new();
        private readonly Dictionary<ushort, IPacketItem> _receiveResponseMap = new();
        private readonly object _receiveLock = new();

        private readonly Dictionary<Type, TaskCompletionSource<IPacketItem>> _ntfWaitMap = new();
        private readonly Dictionary<ushort, TaskCompletionSource<IPacketItem>> _responseWaitMap = new();
   

        public SessionBase Session { get; private set; }

        public readonly int ConnectionId;

        private ushort _currentRequestId = 0;
        private string _token;
        private string _gatewayKey;

        public ProtocolType GetProtocolType() { return ProtocolType.Tcp; }


        public TestSessionClient(SessionServiceBase sessionService , int connectionId)
        {
            _sessionService = sessionService;
            ConnectionId = connectionId;

            _cts = new CancellationTokenSource();
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            if (Session != null)
            {
                Session.Close();
                await _sessionService.OnSessionClosedAsync(Session);
            }
        }

        public ushort GetRequestId()
        {
            _currentRequestId++;
            return _currentRequestId;
        }

        public Task InitSession()
        {
            var localConnection = new LocalConnection(ConnectionId);
            localConnection.Open(OnReceived);
            Session = _sessionService.CreateSession(localConnection);
            _sessionService.OnSessionOpened(Session);

            return Task.CompletedTask;
        }

        private void OnReceived(object packet)
        {
            var packetItem = packet as IPacketItem;
            if (packetItem == null)
                throw new NullReferenceException();

            switch (packetItem.Type())
            {
                // case PacketType.Request:
                //     break;
                case PacketType.Response:
                    packetItem.SetDataBytes(packetItem.GetDataBytes());
                    lock (_receiveLock)
                    {
                        if (_responseWaitMap.TryGetValue(packetItem.GetRequestId(), out var tcs))
                        {
                            tcs.SetResult(packetItem);
                        }
                        else
                        {
                            _receiveResponseMap[packetItem.GetRequestId()] = packetItem;
                        }
                    }
                    break;
                case PacketType.Ntf:
                    packetItem.SetDataBytes(packetItem.GetDataBytes());
                    lock (_receiveLock)
                    {
                        if (_ntfWaitMap.TryGetValue(packetItem.DataType(), out var tcs))
                        {
                            tcs.SetResult(packetItem);
                            _ntfWaitMap.Remove(packetItem.DataType());
                        }
                        else
                        {
                            _receiveNtfList.AddLast(packetItem);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public NetServiceType[] GetServiceTypes()
        {
            return new[] {_sessionService.ServiceType};
        }

        public void AuthorizeToken(string token)
        {
            _gatewayKey = token;
        }

        public void SetEncryptKey(string key)
        {
            _token = key;
        }


        public void ClearReceives()
        {
            lock (_receiveLock)
            {
                _receiveNtfList.Clear();
                _receiveResponseMap.Clear();
            }
        }

        public async Task<IPacketItem> WaitNtfAsync(Type type, int timeoutMilliseconds)
        {
            var ntf = GetNtf(type);
            if (ntf != null)
                return ntf;

            TaskCompletionSource<IPacketItem> tcs;
            lock (_receiveLock)
            {
                var cts = new CancellationTokenSource(timeoutMilliseconds);
                tcs = new TaskCompletionSource<IPacketItem>();
                cts.Token.Register(() => tcs.TrySetCanceled(), false);
                _ntfWaitMap.Add(type, tcs);
            }

            try
            {
                return await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                _ntfWaitMap.Remove(type);
                return null;
            }
        }

        public async Task<IPacketItem> WaitResponseAsync(ushort requestId, int timeoutMilliseconds)
        {
            var response = GetResponse(requestId);
            if (response != null)
                return response;

            TaskCompletionSource<IPacketItem> tcs;
            lock (_receiveLock)
            {
                var cts = new CancellationTokenSource(timeoutMilliseconds);
                tcs = new TaskCompletionSource<IPacketItem>();
                cts.Token.Register(() => tcs.TrySetCanceled(), false);
                _responseWaitMap.Add(requestId, tcs);
            }

            try
            {
                return await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                _responseWaitMap.Remove(requestId);
                return null;
            }
        }

        public IPacketItem GetNtf(Type type)
        {
            lock (_receiveLock)
            {
                var node = _receiveNtfList.First;
                while (node != null)
                {
                    if (node.Value.DataType() == type)
                    {
                        _receiveNtfList.Remove(node);
                        return node.Value;
                    }

                    node = node.Next;
                }

                return null;
            }
        }

        public IPacketItem GetResponse(ushort requestId)
        {
            lock (_receiveLock)
            {
                return _receiveResponseMap.TryGetValue(requestId, out var packet) ? packet : null;
            }
        }

        public async Task<SendPacketResult> SendPacketInternalAsync(RequestReply reply,bool bAssert=false)
        {
            var packetItem = reply.MakePacketItem(GetRequestId());
            var data =  packetItem.GetData();
            if (data is ConnectSessionReq)
            {
            }
            else
            {
                if(Session.Connection.IsAuthenticated)
                    Session.Connection.SetSecretKey(_token);
            }   

            await Session.OnReceiveAsync(packetItem);
            return new SendPacketResult
            {
                SendPacketItem = packetItem,
                ResponsePacketItem = null,
            };
        }
    }
}
