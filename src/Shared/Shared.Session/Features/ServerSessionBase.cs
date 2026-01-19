using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Model;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.PacketModel;
using Shared.Server.Packet.Internal;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Session.Serializer;
using Shared.Session.Utility;
using Shared.TcpNetwork.Base;
using static Shared.Session.Extensions.ReplyExtensions;

namespace Shared.Session.Features
{
    public abstract class ServerSessionBase : SessionBase
    {
        private readonly ILogger<ServerSessionBase> _logger;

        public new readonly ServerSessionServiceBase SessionService;

        public int AppId { get; protected set; }
        public string AppName { get; protected set; }
        public NetServiceType[] AvailableServiceTypes { get; protected set; } = Array.Empty<NetServiceType>();

        private readonly ConcurrentDictionary<int, TaskCompletionSource<(IPacketData, int)>> _receiveTaskMap = new();
        private int _lastRequestId;
        private readonly IPacketSerializer _packetSerializer;

        protected ServerSessionBase(
            ServerSessionServiceBase sessionService,
            IServiceProvider serviceProvider,
            IConnection connection)
            : base(sessionService, serviceProvider, connection)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<ServerSessionBase>>();
            _packetSerializer = serviceProvider.GetRequiredService<SystemTextJsonPacketSerializer>();
            
            SessionService = sessionService;
        }

        private ushort GetRequestId()
        {
            return (ushort) Interlocked.Increment(ref _lastRequestId);
        }

        public void SetLocal()
        {
            AppId = SessionService.AppContext.AppId;
            AppName = SessionService.AppContext.AppName;
            AvailableServiceTypes = SessionService.AvailableServiceTypes;
        }

        public override void OnSessionOpened()
        {
            if (AppId != SessionService.AppContext.AppId)
            {
                SendNtf(MakeNtfReply(new InternalConnectRequestNtf
                {
                    AppId = SessionService.AppContext.AppId,
                    AppName = SessionService.AppContext.AppName,
                    AvailableServiceTypes = SessionService.AvailableServiceTypes,
                }));
            }

            base.OnSessionOpened();
        }

        public override void Close()
        {
            SessionService.RemoveServerSession(this);

            base.Close();
        }

        public override Task OnSessionClosedAsync()
        {
            SessionService.RemoveServerSession(this);

            return base.OnSessionClosedAsync();
        }

        public Task PingCheckAsync(PingCheckNtf _)
        {
            SendNtf(MakeNtfReply<InternalPingNtf>());

            return Task.CompletedTask;
        }

        public Task InternalPingAsync(InternalPingNtf _)
        {
            return Task.CompletedTask;
        }

        public override async Task OnReceiveAsync(IPacketItem packetItem)
        {
            Flags &= ~SessionFlags.IsPing;

            Task task;
            switch (packetItem.Type())
            {
                case PacketType.Request:
                    task = OnRequestReceiveAsync(packetItem);
                    break;
                case PacketType.Response:
                    task = OnResponseReceiveAsync(packetItem);
                    break;
                case PacketType.Ntf:
                    task = OnNtfReceiveAsync(packetItem);
                    break;
                default:
                    LogErrorPacket(packetItem, "OnReceive, invalid packet type");
                    task = Task.CompletedTask;
                    break;
            }

            await task.ConfigureAwait(false);
        }

        protected override async Task<ResultCode> OnRequestReceiveAsync(IPacketItem packetItem)
        {
            var handlerData = PacketHandlerInfoTableMap.Get(SessionService.SessionType, packetItem.GetHeaderData());
            if (handlerData == null)
            {
                LogErrorPacket(packetItem, "OnRequestReceiveAsync, handler not found");
                return ResultCode.NotSupportedPacket;
            }

            if (handlerData.ParameterType != packetItem.DataType())
            {
                LogErrorPacket(packetItem, "OnRequestReceiveAsync, invalid handler DataType");
                return ResultCode.NotSupportedPacket;
            }

            var checkReply = await OnCheckPacketAsync(handlerData, packetItem);
            if (checkReply != null)
            {
                SendResponse(checkReply, packetItem);
                return (ResultCode) checkReply.Result;
            }

            ResponseReply reply;
            try
            {
                reply = await InvokeRequestSafeAsync(handlerData, packetItem);
            }
            catch (Exception e)
            {
                LogErrorPacket(e, packetItem, "OnRequestReceiveAsync, invoke exception");

                reply = MakeResReply(handlerData.ReturnType, ResultCode.ServerError);
            }

            if (reply == null)
            {
                LogErrorPacket(packetItem, "OnRequestReceiveAsync, reply is null");
                return ResultCode.ServerError;
            }

            SendResponse(reply, packetItem);

            return ResultCode.Success;
        }

        private Task OnResponseReceiveAsync(IPacketItem packetItem)
        {
            var requestId = packetItem.GetRequestId();
            if (_receiveTaskMap.TryGetValue(requestId, out var receiveTask) == false)
            {
                LogErrorPacket(packetItem, "OnResponseReceiveAsync, invalid requestId");
                return Task.CompletedTask;
            }
            _receiveTaskMap.TryRemove(requestId, out _);
            receiveTask?.SetResult((packetItem.GetData(), packetItem.GetResult()));

            return Task.CompletedTask;
        }

        private async Task OnNtfReceiveAsync(IPacketItem packetItem)
        {
            var handlerData = PacketHandlerInfoTableMap.Get(SessionService.SessionType, packetItem.GetHeaderData());
            if (handlerData == null)
            {
                LogErrorPacket(packetItem, "OnNtfReceiveAsync, handler not found");
                return;
            }

            if (handlerData.ParameterType != packetItem.DataType())
            {
                LogErrorPacket(packetItem, "OnNtfReceiveAsync, invalid handler DataType");
                return;
            }

            var checkReply = await OnCheckPacketAsync(handlerData, packetItem);
            if (checkReply != null)
            {
                SendResponse(checkReply, packetItem);
                return;
            }

            await InvokeNtfSafeAsync(handlerData, packetItem);
        }

        protected override Task<ResponseReply> OnCheckPacketAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            if (packetItem.Type() == PacketType.Request)
            {
                if (AppId == 0)
                {
                    return Task.FromResult(MakeResReply(handlerData.ReturnType, ResultCode.NotAuthorized));
                }
            }

            return base.OnCheckPacketAsync(handlerData, packetItem);
        }

        public async Task<InternalPacketResult<T>> SendRequestAsync<T>(RequestReply reply) where T : class, IPacketData
        {
            var packetItem = reply.MakePacketItem(GetRequestId());

            var receiveTask = new TaskCompletionSource<(IPacketData, int)>();
            _receiveTaskMap.TryAdd(packetItem.GetRequestId(), receiveTask);

            SendPacket(packetItem);

            var data = await receiveTask.Task;
            if (data.Item2 != 0)
            {
                LogErrorPacket(packetItem, $"SendRequestAsync, result is not success: {(ResultCode)data.Item2}");
                return new InternalPacketResult<T>(data.Item2, null);
            }

            return new InternalPacketResult<T>((int) ResultCode.Success, (T) data.Item1);
        }

        protected override void SendPacket(IPacketItem packetItem)
        {
            var bytes = _packetSerializer.Serialize(packetItem);
            packetItem.SetDataBytes(bytes);
            
            base.SendPacket(packetItem);
        }

        protected override void LogPacket(IPacketItem packetItem, string message)
        {
            if (SessionService.Settings.EnableTracePacketLog == false)
                return;

            _logger.LogInformation(
                "ServerSession Message: {Message} | AppId({AppId}) AppName({AppName}) SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), RequestId({RequestId}), Result({Result})",
                message, AppId, AppName, SessionService.SessionType.Name, SessionId, packetItem.DataType().Name, packetItem.Header(),
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected override void LogWarningPacket(IPacketItem packetItem, string message)
        {
            _logger.LogWarning(
                "ServerSession Message: {Message} | AppId({AppId}) AppName({AppName}) SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                message, AppId, AppName, SessionService.SessionType.Name, SessionId, packetItem.DataType().Name, packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected override void LogErrorPacket(IPacketItem packetItem, string message)
        {
            _logger.LogCritical(
                "ServerSession Message: {Message} | AppId({AppId}) AppName({AppName}) SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                message, AppId, AppName, SessionService.SessionType.Name, SessionId, packetItem.DataType().Name, packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected override void LogErrorPacket(Exception e, IPacketItem packetItem, string message)
        {
            _logger.LogCritical(e,
                "ServerSession Message: {Message} | AppId({AppId}) AppName({AppName}) SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                message, AppId, AppName, SessionService.SessionType.Name, SessionId, packetItem?.DataType()?.Name, packetItem?.Header(),
                packetItem?.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem?.GetRequestId(), packetItem?.GetResultCode());
        }
    }
}