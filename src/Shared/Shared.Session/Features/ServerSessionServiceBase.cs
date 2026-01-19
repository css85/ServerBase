using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Model;
using Shared.Network.Base;
using Shared.Network.Connection;
using Shared.Network.Utility;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.Server.Define;
using Shared.Server.Packet.Internal;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Session.Extensions;
using Shared.Session.Services;
using Shared.Session.Settings;
using Shared.TcpNetwork.Transport;
using static Shared.Session.Extensions.ReplyExtensions;

namespace Shared.Session.Features
{
    public enum ServerSessionConnectResult
    {
        Connecting,
        Stopped,
        Failed,
        Success,
    }

    public abstract class ServerSessionServiceBase : SessionServiceBase
    {
        public readonly RedisRepositoryService RedisRepo;
        public readonly NetServiceType[] AvailableServiceTypes;

        private readonly ILogger<ServerSessionServiceBase> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly SessionManagementService _sessionManagementService;
        private readonly ITcpSetting _tcpConnectionSettings;
        private readonly ChangeableSettings<ServerSessionSettings> _serverSessionSettings;

        public new ServerSessionSettings Settings => _serverSessionSettings.Value;

        private readonly ConcurrentDictionary<int, ServerSessionBase> _sessionMap = new();
        private readonly ConcurrentDictionary<int, ServerSessionBase> _serverToSessionMap = new();
        private readonly ConcurrentDictionary<NetServiceType, List<ServerSessionBase>> _serverSessionsMap = new();
                

        private readonly TaskWorker<PacketWorkerItem> _localPacketTaskWorker;

        private readonly Random _random = new();

        public SessionListenerBase SessionListener;
        private ServerSessionBase _localServerSession;

        private readonly Dictionary<int, IConnection> _connectionMap;

        private readonly object _sessionLock = new();

        protected ServerSessionServiceBase(
            ILogger<ServerSessionServiceBase> logger,
            IServiceProvider serviceProvider,
            RedisRepositoryService redisRepo,
            SessionManagementService sessionManagementService,
            Type sessionType,
            NetServiceType[] availableServiceTypes
        ) : base(logger, serviceProvider, NetServiceType.Internal, sessionType)
        {
            _logger = logger;
            RedisRepo = redisRepo;
            _sessionManagementService = sessionManagementService;
            AvailableServiceTypes = availableServiceTypes;

            _hostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            _serverSessionSettings = serviceProvider.GetRequiredService<ChangeableSettings<ServerSessionSettings>>();
            _tcpConnectionSettings= serviceProvider.GetRequiredService<TcpConnectionSettingBase>();

            _localPacketTaskWorker = new TaskWorker<PacketWorkerItem>(32, OnLocalTaskReceivedAsync);
            _connectionMap = new Dictionary<int, IConnection>();
        }

        public override Task StartAsync()
        {
            var sessionListenerType = typeof(ServerSessionListenerService<>).MakeGenericType(GetType());
            var sessionListener = ServiceProvider.GetService(sessionListenerType);
            if (sessionListener != null)
                SessionListener = (SessionListenerBase)sessionListener;

            AddLocalConnection();

            return base.StartAsync();
        }

        public override async Task OnStopAsync()
        {
            _localPacketTaskWorker.Dispose();

            await base.OnStopAsync();
        }

        private void AddLocalConnection()
        {
            var localConnection = new LocalConnection(GetSessionId());
            localConnection.Received += OnLocalConnectionReceived;
            localConnection.Open(OnLocalPacketItemReceived);
            _localServerSession = (ServerSessionBase) CreateSession(localConnection);
            _localServerSession.SetLocal();
            AddServerSession(_localServerSession);
        }

        private void OnLocalConnectionReceived(IConnection connection, object packetItem)
        {
            OnLocalPacketItemReceived(packetItem);
        }

        private void OnLocalPacketItemReceived(object item)
        {
            _localPacketTaskWorker.Enqueue(new PacketWorkerItem(0, item));
        }

        private Task OnLocalTaskReceivedAsync(PacketWorkerItem item)
        {
            var packetItem = item.Packet as IPacketItem;
            return _localServerSession.OnReceiveAsync(packetItem);
        }

        protected virtual void OnServerSessionAdded(ServerSessionBase session)
        {
        }

        private ServerSessionBase GetLocalServerSession()
        {
            return _serverToSessionMap[AppContext.AppId];
        }

        public void AddServerSession(ServerSessionBase session)
        {
            lock (_sessionLock)
            {
                if (session.AppId == 0)
                {
                    _logger.LogWarning("Invalid add ServerSession: {AppName}, {AppId}, {SessionId}",
                        session.AppName, session.AppId, session.SessionId);
                    session.Close();
                    return;
                }

                if (_serverToSessionMap.TryAdd(session.AppId, session) == false)
                {
                    _logger.LogWarning("Already connected ServerSession, close new ServerSession: {AppName}, {AppId}, {SessionId}",
                        session.AppName, session.AppId, session.SessionId);
                    session.Close();
                    return;
                }
                _sessionMap.TryAdd(session.SessionId, session);

                foreach (var serviceType in session.AvailableServiceTypes)
                {
                    if (_serverSessionsMap.TryGetValue(serviceType, out var sessions) == false)
                    {
                        sessions = new List<ServerSessionBase>();
                        _serverSessionsMap.TryAdd(serviceType, sessions);
                    }
                    sessions.Add(session);
                }

                OnServerSessionAdded(session);

                _logger.LogInformation("Add ServerSession: {AppName}, {AppId}, {SessionId}",
                    session.AppName, session.AppId, session.SessionId);
            }
        }

        public void RemoveServerSession(ServerSessionBase session)
        {
            lock (_sessionLock)
            {
                if (_sessionMap.ContainsKey((session.SessionId)))
                {
                    foreach (var serviceType in session.AvailableServiceTypes)
                    {
                        if (_serverSessionsMap.TryGetValue(serviceType, out var sessions))
                        {
                            if (sessions.Remove(session))
                            {
                                _serverToSessionMap.TryRemove(session.AppId, out _);

                                _logger.LogInformation(
                                    "Remove ServerSession Service: {AppName}, {AppId}, {SessionId}, {ServiceType}",
                                    session.AppName, session.AppId, session.SessionId, serviceType);
                            }

                            if (sessions.Count == 0)
                                _serverSessionsMap.TryRemove(serviceType, out _);
                        }
                    }

                    _serverToSessionMap.TryRemove(session.AppId, out _);

                    _logger.LogInformation(
                        "Remove ServerSession: {AppName}, {AppId}, {SessionId}",
                        session.AppName, session.AppId, session.SessionId);
                }
            }
        }

        //public async Task<ServerSessionConnectResult> ConnectAsync(AppServerDataModel server)
        //{
        //    if (_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
        //        return ServerSessionConnectResult.Stopped;
        //    if (_hostApplicationLifetime.ApplicationStopped.IsCancellationRequested)
        //        return ServerSessionConnectResult.Stopped;

        //    _logger.LogInformation("Start connect ServerSession: [{AppId}, {AppName}]", server.AppId, server.AppName);
        //    if (_serverConnectMap.TryAdd(server.AppId, server) == false)
        //    {
        //        _logger.LogWarning("Already connecting ServerSession: [{AppId}, {AppName}]", server.AppId, server.AppName);
        //        return ServerSessionConnectResult.Connecting;
        //    }

        //    var connectTcs = new TaskCompletionSource<bool>();
        //    await using var ctr =
        //        _hostApplicationLifetime.ApplicationStopping.Register(() => connectTcs.TrySetCanceled(), false);
        //    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        //    timeoutCts.Token.Register(() => connectTcs.TrySetCanceled(), false);

        //    var tcpConnector = new TcpConnector();
        //    tcpConnector.Connected += (_, socket) =>
        //    {
        //        try
        //        {
        //            var tcpConnection = new TcpConnection(_logger.CreateTcpConnectionLogger(), _tcpConnectionSettings,
        //                socket, GetSessionId());
        //            tcpConnection.Received += SessionListener.OnReceived;
        //            tcpConnection.SerializeErrored += SessionListener.OnSerializeErrored;
        //            tcpConnection.Opened += SessionListener.OnConnectionOpened;
        //            tcpConnection.Closed += SessionListener.OnConnectionClosed;
        //            if (_connectionMap.TryAdd(server.AppId, tcpConnection))
        //                tcpConnection.Open();

        //            connectTcs.TrySetResult(tcpConnection.IsConnect);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.LogWarning(e,
        //                "ServerSession connection open failed. {AppGroupName}({AppGroupId}), {AppName}({AppId}), {InternalHost}:{InternalPort}",
        //                server.AppGroupName, server.AppGroupId, server.AppName, server.AppId, server.InternalHost,
        //                server.InternalPort);

        //            connectTcs.TrySetResult(false);
        //        }
        //    };
        //    tcpConnector.ConnectFailed += (_, error) =>
        //    {
        //        _logger.LogWarning(
        //            "ServerSession connect failed. Reason({Reason}), {AppGroupName}({AppGroupId}), {AppName}({AppId}), {InternalHost}:{InternalPort}",
        //            error, server.AppGroupName, server.AppGroupId, server.AppName, server.AppId,
        //            server.InternalHost,
        //            server.InternalPort);
        //        connectTcs.TrySetResult(false);
        //    };
        //    try
        //    {
        //        var endPoint = new IPEndPoint(IPAddress.Parse(server.InternalHost), server.InternalPort);

        //        using var tcpConnectCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token,
        //            _hostApplicationLifetime.ApplicationStopping);
        //        await tcpConnector.ConnectAsync(endPoint, tcpConnectCts.Token);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogWarning(e,
        //            "ServerSession connect failed. {AppGroupName}({AppGroupId}), {AppName}({AppId}), {InternalHost}:{InternalPort}",
        //            server.AppGroupName, server.AppGroupId, server.AppName, server.AppId, server.InternalHost,
        //            server.InternalPort);
        //        connectTcs.TrySetResult(false);
        //    }

        //    var isConnect = await connectTcs.Task;

        //    _connectionMap.TryGetValue(server.AppId, out var connection);
        //    if (isConnect == false)
        //    {
        //        connection?.Close();
        //        _connectionMap.Remove(server.AppId);
        //        _serverConnectMap.TryRemove(server.AppId, out _);

        //        return ServerSessionConnectResult.Failed;
        //    }

        //    _serverConnectMap.TryRemove(server.AppId, out _);
        //    return ServerSessionConnectResult.Success;
        //}

        public IEnumerable<ServerSessionBase> GetServerSessions(NetServiceType serviceType)
        {
            return _serverSessionsMap.TryGetValue(serviceType, out var serverSessions)
                ? serverSessions
                : Array.Empty<ServerSessionBase>();
        }

        /// <summary>
        /// NetServiceType에 해당하는 서버에 패킷전송후 결과를 기다림
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷 (패킷을 전송하지 못했으면 null)</returns>
        public async Task<InternalPacketResult<TPacketResult>> SendAsync<TPacketResult>(NetServiceType serviceType, RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            var serverSession = GetServerSession(serviceType);
            if (serverSession == null)
            {
                OnServerSessionEmpty(reply.GetHeaderData().Header, serviceType);
                return null;
            }

            return await SendByServerSessionAsync<TPacketResult>(serverSession, reply);
        }

        /// <summary>
        /// NetServiceType에 해당하는 서버에 패킷전송
        /// </summary>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>패킷전송 여부</returns>
        public async Task<bool> SendAsync(NetServiceType serviceType, NtfReply reply)
        {
            var serverSession = GetServerSession(serviceType);
            if (serverSession == null)
            {
                OnServerSessionEmpty(reply.GetHeaderData().Header, serviceType);
                return false;
            }

            await SendByServerSessionAsync(serverSession, reply);
            return true;
        }

        /// <summary>
        /// NetServiceType에 해당하는 모든 서버에 패킷전송후 결과를 기다림
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷들</returns>
        public async Task<InternalPacketResult<TPacketResult>[]> SendAllAsync<TPacketResult>(NetServiceType serviceType, RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            if (_serverSessionsMap.TryGetValue(serviceType, out var serverSessions))
            {
                if (serverSessions != null && serverSessions.Count > 0)
                {
                    var results = new InternalPacketResult<TPacketResult>[serverSessions.Count];
                    var index = 0;
                    foreach (var session in serverSessions)
                    {
                        results[index++] = await SendByServerSessionAsync<TPacketResult>(session, reply);
                    }

                    return results;
                }
            }

            // OnServerSessionEmpty(reply.GetPacket().Header, serviceType);
            return Array.Empty<InternalPacketResult<TPacketResult>>();
        }

        /// <summary>
        /// NetServiceType에 해당하는 모든 서버에 패킷전송
        /// </summary>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        public async Task SendAllAsync(NetServiceType serviceType, NtfReply reply)
        {
            if (_serverSessionsMap.TryGetValue(serviceType, out var serverSessions))
            {
                if (serverSessions != null && serverSessions.Count > 0)
                {
                    foreach (var session in serverSessions)
                        await SendByServerSessionAsync(session, reply);

                    // return;
                }
            }

            // OnServerSessionEmpty(reply.GetPacket().Header, serviceType);
        }


        /// <summary>
        /// NetServiceType에 해당하는 현재 서버에 패킷전송후 결과를 기다림
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷 (패킷을 전송하지 못했으면 null)</returns>
        public async Task<InternalPacketResult<TPacketResult>> SendRequestLocalAsync<TPacketResult>(NetServiceType serviceType, RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            var localServerSession = GetLocalServerSession();
            if (localServerSession.AvailableServiceTypes.Contains(serviceType) == false)
                return null;

            return await SendRequestLocalAsync<TPacketResult>(reply);
        }

        /// <summary>
        /// NetServiceType에 해당하는 현재 서버에 패킷전송
        /// </summary>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        public ValueTask SendNtfLocalAsync(NetServiceType serviceType, NtfReply reply)
        {
            var localServerSession = GetLocalServerSession();
            if (localServerSession.AvailableServiceTypes.Contains(serviceType) == false)
                return ValueTask.CompletedTask;

            return SendNtfLocalAsync(reply);
        }

        /// <summary>
        /// 다른 모든서버에 패킷전송후 결과를 기다림
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷 (패킷을 전송하지 못했으면 null)</returns>
        public async Task<InternalPacketResult<TPacketResult>[]> SendServerAllAsync<TPacketResult>(RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            var serverSessions = _serverToSessionMap.Values;
            if (serverSessions.Count == 0)
                return null;

            var results = new InternalPacketResult<TPacketResult>[serverSessions.Count - 1];
            var index = 0;

            foreach (var serverSession in serverSessions)
            {
                if (serverSession.AppId != AppContext.AppId)
                    results[index++] = await SendByServerSessionAsync<TPacketResult>(serverSession, reply);
            }

            return results;
        }

        /// <summary>
        /// 다른 모든서버에 패킷전송
        /// </summary>
        /// <param name="reply">보낼 패킷</param>
        public async Task SendServerAllAsync(NtfReply reply)
        {
            var serverSessions = _serverToSessionMap.Values;
            if (serverSessions.Count > 0)
            {
                foreach (var serverSession in serverSessions)
                {
                    if (serverSession.AppId != AppContext.AppId)
                    {
                        await SendByServerSessionAsync(serverSession, reply);
                    }
                }
            }
        }


        /// <summary>
        /// AppId에 해당하는 서버에 패킷전송후 결과를 기다림
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="appId">보낼 서버 AppId</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷 (패킷을 전송하지 못했으면 null)</returns>
        public async Task<InternalPacketResult<TPacketResult>> SendServerAsync<TPacketResult>(int appId, RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            var serverSession = GetServerSession(appId);
            if (serverSession == null)
            {
                OnServerSessionNotFound(reply.GetHeaderData().Header, appId);
                return null;
            }

            return await SendByServerSessionAsync<TPacketResult>(serverSession, reply);
        }

        /// <summary>
        /// AppId에 해당하는 서버에 패킷전송
        /// </summary>
        /// <param name="appId">보낼 서버 AppId</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>패킷전송 여부</returns>
        public async Task<bool> SendServerAsync(int appId, NtfReply reply)
        {
            var serverSession = GetServerSession(appId);
            if (serverSession == null)
            {
                OnServerSessionNotFound(reply.GetHeaderData().Header, appId);
                return false;
            }

            await SendByServerSessionAsync(serverSession, reply);
            return true;
        }

        /// <summary>
        /// 해당 유저가 있는 서버에 패킷전송후 결과를 기다림 (유저가 오프라인이면 NetServiceType 서버중 하나로)
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="userSeq">userSeq</param>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷 (패킷전송 안되었으면 null)</returns>
        public async Task<InternalPacketResult<TPacketResult>> SendByUserAsync<TPacketResult>(NetServiceType serviceType, long userSeq,
            RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            var serverSession = await GetServerSessionByUserAsync(serviceType, userSeq);
            if (serverSession == null)
                serverSession = GetServerSession(serviceType);

            if (serverSession == null)
            {
                OnServerSessionEmpty(reply.GetHeaderData().Header, serviceType);
                //TODO: 로깅
                return null;
            }

            return await SendByServerSessionAsync<TPacketResult>(serverSession, reply);
        }

        /// <summary>
        /// 해당 유저가 있는 서버에 패킷전송 (유저가 오프라인이면 NetServiceType 서버중 하나로)
        /// </summary>
        /// <param name="userSeq">대상 유저</param>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>패킷전송 여부</returns>
        public async Task<bool> SendByUserAsync(NetServiceType serviceType, long userSeq, NtfReply reply)
        {
            var serverSession = await GetServerSessionByUserAsync(serviceType, userSeq);
            if (serverSession == null)
                serverSession = GetServerSession(serviceType);

            if (serverSession == null)
            {
                OnServerSessionEmpty(reply.GetHeaderData().Header, serviceType);
                return false;
            }

            await SendByServerSessionAsync(serverSession, reply);
            return true;
        }

        /// <summary>
        /// 해당 유저가 있는 서버에 패킷전송후 결과를 기다림  (유저가 오프라인이면 패킷전송 안됨)
        /// </summary>
        /// <typeparam name="TPacketResult">결과 패킷의 타입</typeparam>
        /// <param name="userSeq">userSeq</param>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>결과패킷 (유저가 오프라인이면 null)</returns>
        public async Task<InternalPacketResult<TPacketResult>> SendByUserOnlyAsync<TPacketResult>(NetServiceType serviceType, long userSeq,
            RequestReply reply)
            where TPacketResult : class, IPacketData
        {
            var serverSession = await GetServerSessionByUserAsync(serviceType, userSeq);
            if (serverSession == null)
                return null;

            return await SendByServerSessionAsync<TPacketResult>(serverSession, reply);
        }

        /// <summary>
        /// 해당 유저가 있는 서버에 패킷전송 (유저가 오프라인이면 패킷전송 안됨)
        /// </summary>
        /// <param name="userSeq">대상 유저</param>
        /// <param name="serviceType">보낼 NetServiceType</param>
        /// <param name="reply">보낼 패킷</param>
        /// <returns>패킷전송 여부</returns>
        public async Task<bool> SendByUserOnlyAsync(NetServiceType serviceType, long userSeq, NtfReply reply)
        {
            var serverSession = await GetServerSessionByUserAsync(serviceType, userSeq);
            if (serverSession == null)
                return false;

            await SendByServerSessionAsync(serverSession, reply);
            return true;
        }

        /// <summary>
        /// 해당 유저가 있는 서버의 유저에게 패킷 전송 (유저가 오프라인이면 패킷전송 안됨)
        /// </summary>
        /// <param name="userSeq">패킷을 전달받을 유저</param>
        /// <param name="serviceType">NetServiceType</param>
        /// <param name="reply">전달할 패킷</param>
        public async Task SendForwardAsync(NetServiceType serviceType, long userSeq, NtfReply reply)
        {
            var serverSession = await GetServerSessionByUserAsync(serviceType, userSeq);
            if (serverSession == null)
                return;

            if (serverSession.AppId == AppContext.AppId)
            {
                await SendForwardLocalAsync(serviceType, userSeq, reply);
                return;
            }

            var packet = reply.MakePacketItem();
            serverSession.SendNtf(MakeNtfReply(new InternalForwardNtf
            {
                UserSeq = userSeq,
                ServiceType = serviceType,
                Major = packet.Header().Major,
                Minor = packet.Header().Minor,
                Body = JsonTextSerializer.Serialize(packet.GetData(), packet.DataType()),
            }));
        }

        /// <summary>
        /// 현재 서버의 해당 유저에게 패킷 전송 (유저가 오프라인이면 패킷전송 안됨)
        /// </summary>
        /// <param name="userSeq">패킷을 전달받을 유저</param>
        /// <param name="serviceType">NetServiceType</param>
        /// <param name="reply">전달할 패킷</param>
        public Task SendForwardLocalAsync(NetServiceType serviceType, long userSeq, NtfReply reply)
        {
            var session = _sessionManagementService.GetSessionByUser(serviceType, userSeq);
            if (session != null)
            {
                return session.SendForwardNtfAsync(reply).AsTask();
            }

            return Task.CompletedTask;
        }

        private Task<InternalPacketResult<T>> SendByServerSessionAsync<T>(ServerSessionBase serverSession, RequestReply reply) where T : class, IPacketData
        {
            if (serverSession.AppId == AppContext.AppId)
                return SendRequestLocalAsync<T>(reply);

            return serverSession.SendRequestAsync<T>(reply);
        }

        private ValueTask SendByServerSessionAsync(ServerSessionBase serverSession, NtfReply reply)
        {
            if (serverSession.AppId == AppContext.AppId)
                return SendNtfLocalAsync(reply);

            serverSession.SendNtf(reply);

            return ValueTask.CompletedTask;
        }

        private async Task<InternalPacketResult<T>> SendRequestLocalAsync<T>(RequestReply reply) where T : class, IPacketData
        {
            var localServerSession = GetLocalServerSession();

            var sendPacketItem = reply.MakePacketItem();
            var receiveReply = await localServerSession.InvokeRequestSafeAsync(sendPacketItem);
            var receivePacketItem = receiveReply?.MakePacketItem();
            if (receivePacketItem == null)
                return new InternalPacketResult<T>((int)ResultCode.InternalFail, null);

            if (receiveReply.GetHeaderData().PacketType == PacketType.Response)
            {
                return new InternalPacketResult<T>(receivePacketItem.GetResult(), (T) receivePacketItem.GetData());
            }
            else
            {
                return new InternalPacketResult<T>(ResultCode.Success, (T) receivePacketItem.GetData());
            }
        }

        private async Task<InternalPacketResult<T>> SendNtfLocalAsync<T>(NtfReply reply) where T : class, IPacketData
        {
            var localServerSession = GetLocalServerSession();

            var sendPacketItem = reply.MakePacketItem();
            var result = await localServerSession.InvokeNtfSafeAsync(sendPacketItem);
            if (result == false)
                return new InternalPacketResult<T>((int)ResultCode.InternalFail, null);

            return new InternalPacketResult<T>(ResultCode.Success, null);
        }

        public async ValueTask SendNtfLocalAsync(NtfReply reply)
        {
            var localServerSession = GetLocalServerSession();
            await localServerSession.InvokeNtfSafeAsync(reply.MakePacketItem());
        }

        public ServerSessionBase GetServerSession(NetServiceType serviceType)
        {
            if (_serverSessionsMap.TryGetValue(serviceType, out var serverSessions))
            {
                // TODO: 로드밸런싱
                // TODO: 일단 랜덤으로, 나중에 서버 부하로 계산
                if (serverSessions.Count == 0)
                    return null;

                return serverSessions[_random.Next() % serverSessions.Count];
            }

            return null;
        }

        public ServerSessionBase GetServerSession(int appId)
        {
            return _serverToSessionMap.TryGetValue(appId, out var serverSession) ? serverSession : null;
        }

        public async Task<ServerSessionBase> GetServerSessionByUserAsync(NetServiceType serviceType, long userSeq)
        {
            var session = _sessionManagementService.GetSessionByUser(serviceType, userSeq);
            if (session != null)
                return GetLocalServerSession();
            
            return await GetServerSessionByUserAsync(RedisKeys.ServiceSessionKey(serviceType), userSeq);
        }

        private async Task<ServerSessionBase> GetServerSessionByUserAsync(string sessionRedisKey, long userSeq)
        {
            var sessionRedis = RedisRepo.GetDb(RedisDatabase.Session);
            var appIdValue = await sessionRedis.HashGetAsync(sessionRedisKey, userSeq);
            if (appIdValue.HasValue)
            {
                var appId = (int)appIdValue;
                if (_serverToSessionMap.TryGetValue(appId, out var serverSession))
                {
                    return serverSession;
                }
            }

            return null;
        }

        private void OnServerSessionNotFound(PacketHeader packetHeader, NetServiceType serviceType)
        {
            _logger.LogCritical("Target ServerSession not found. ({PacketHeader} {ServiceType})", packetHeader, serviceType);
        }

        private void OnServerSessionNotFound(PacketHeader packetHeader, int appId)
        {
            _logger.LogCritical("Target ServerSession not found. ({PacketHeader} {AppId})", packetHeader, appId);
        }

        private void OnServerSessionEmpty(PacketHeader packetHeader, NetServiceType serviceType)
        {
            _logger.LogCritical("Target ServerSession not found. ({PacketHeader} {ServiceType})", packetHeader, serviceType);
        }
    }
}
