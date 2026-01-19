using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SampleGame.Shared;
using SampleGame.Shared.Common;
using Elastic.Apm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.PacketModel;
using Shared.Session.Data;
using Shared.Session.Services;
using Shared.Session.Utility;
using static Shared.Session.Extensions.ReplyExtensions;

namespace Shared.Session.Base
{
    [Flags]
    public enum SessionFlags
    {
        None = 0,
        IsPing = 0x01,
    }

    public abstract class SessionBase
    {
        private readonly ILogger<SessionBase> _logger;
        protected readonly IServiceProvider ServiceProvider;
        public readonly SessionServiceBase SessionService;
        public readonly int SessionId;
        public DateTime LastReceivedTime => Connection.GetLastReceiveTime();
        public SessionFlags Flags { get; protected set; }
        public bool IsConnected => Connection.IsConnected();

        public readonly IConnection Connection;
        
        private readonly Stopwatch _invokeStopwatch = new();
        protected readonly SessionManagementService _sessionManagementService;
        protected SessionBase(
            SessionServiceBase sessionService,
            IServiceProvider serviceProvider,
            IConnection connection
        )
        {
            ServiceProvider = serviceProvider;

            _logger = serviceProvider.GetRequiredService<ILogger<SessionBase>>();
            _sessionManagementService = serviceProvider.GetRequiredService<SessionManagementService>();
            SessionService = sessionService;

            SessionId = connection.GetId();
            Connection = connection;
            Flags = SessionFlags.None;
        }

        public virtual void OnSessionOpened()
        {
        }

        public virtual Task OnSessionClosedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void Close()
        {
            Connection.Close();
        }

        public virtual Task<ResponseReply> PingAsync(PingReq _)
        {
            return Task.FromResult(MakeResReply<PingRes>());
        }

        public virtual Task KickAsync()
        {
            try
            {
                SendNtf(MakeNtfReply(new KickNtf
                {
                    NetServiceType = (byte) SessionService.ServiceType,
                    KickReason = 0,
                }));
            }
            catch
            {
            }
            finally
            {
                Close();
            }
            return Task.CompletedTask;
        }

        public virtual void SendPing()
        {
            Flags &= SessionFlags.IsPing;
            SendNtf(MakeNtfReply(new PingCheckNtf
            {
                NetServiceType = (byte)SessionService.ServiceType,
            }));
        }

        public void SendNtf(NtfReply reply)
        {
            var packetItem = reply.MakePacketItem();
            SendPacket(packetItem);
        }

        public virtual ValueTask SendForwardNtfAsync(NtfReply reply)
        {
            var packetItem = reply.MakePacketItem();
            SendPacket(packetItem);
            return ValueTask.CompletedTask;
        }

        protected void SendResponse(ResponseReply reply, IPacketItem originPacket = null)
        {
            var packetItem = originPacket != null
                            ? reply.MakePacketItem(originPacket.GetRequestId())
                            : reply.MakePacketItem();
            SendPacket(packetItem);
        }
        protected virtual void SendPacket(IPacketItem packetItem)
        {
            LogPacket(packetItem, "SendPacket");

            try
            {
                Connection.Send(packetItem);
            }
            catch (OperationCanceledException)
            {
                Close();
            }
            catch (InvalidOperationException)
            {
                Close();
            }
            catch (Exception e)
            {
                LogErrorPacket(e, packetItem, "SendPacket Failed.");
                Close();
            }
        }

        protected virtual Task<ResponseReply> OnCheckPacketAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            return Task.FromResult((ResponseReply) null);
        }

        public virtual Task OnReceiveAsync(IPacketItem packetItem)
        {
            switch (packetItem.Type())
            {
                case PacketType.Request:
                    return OnRequestReceiveAsync(packetItem);
                default:
                    LogErrorPacket(packetItem, "OnReceive, invalid packet type");
                    return Task.CompletedTask;
            }
        }

        protected virtual async Task<ResultCode> OnRequestReceiveAsync(IPacketItem packetItem)
        {
            var handlerData = PacketHandlerInfoTableMap.Get(SessionService.SessionType, packetItem.GetHeaderData());

            if (handlerData == null)
            {
                LogErrorPacket(packetItem, "OnRequestReceive, handler not found");
                await KickAsync();
                return ResultCode.NotSupportedPacket;
            }

            if (handlerData.ParameterType != packetItem.DataType())
            {
                LogErrorPacket(packetItem, "OnRequestReceive, invalid handler DataType");
                return ResultCode.NotSupportedPacket;
            }

            var checkReply = await OnCheckPacketAsync(handlerData, packetItem);
            if (checkReply != null)
            {
                SendResponse(checkReply, packetItem);
                return (ResultCode) checkReply.Result;
            }

            var reply = await InvokeRequestSafeAsync(handlerData, packetItem);
            if (reply == null)
            {
                LogErrorPacket(packetItem, "OnRequestReceive, reply is null");
                return ResultCode.ServerError;
            }

            SendResponse(reply, packetItem);
            return ResultCode.Success;
        }

        public async Task<ResponseReply> InvokeRequestSafeAsync(IPacketItem packetItem)
        {
            try
            {
                return await InvokeRequestAsync(packetItem);
            }
            catch (Exception e)
            {
                LogErrorPacket(e, packetItem, "InvokeRequestSafeAsync, invoke exception");
                var handlerData = PacketHandlerInfoTableMap.Get(SessionService.SessionType, packetItem.GetHeaderData());
                return MakeResReply(handlerData.ReturnType, ResultCode.ServerError, packetItem.GetRequestId());
            }
        }

        private Task<ResponseReply> InvokeRequestAsync(IPacketItem packetItem)
        {
            var handlerData = PacketHandlerInfoTableMap.Get(SessionService.SessionType, packetItem.GetHeaderData());
            if (handlerData == null)
            {
                LogErrorPacket(packetItem, "InvokeRequestAsync, handler not found");
                return Task.FromResult((ResponseReply) MakeNoReply());
            }

            var result = handlerData.MethodInfo.Invoke(this, new object[] { packetItem.GetData() });
            if (result == null)
            {
                LogErrorPacket(packetItem, "InvokeRequestAsync, handler method not found");
                return Task.FromResult((ResponseReply) MakeNoReply());
            }

            return (Task<ResponseReply>)result;
        }

        public async Task<ResponseReply> InvokeRequestSafeAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            try
            {
                return await InvokeRequestAsync(handlerData, packetItem);
            }
            catch (Exception e)
            {
                LogErrorPacket(e, packetItem, "InvokeRequestSafeAsync, invoke exception");
                return MakeResReply(handlerData.ReturnType, ResultCode.ServerError, packetItem.GetRequestId());
            }
        }

        private Task<ResponseReply> InvokeRequestAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            try
            {
                var result = handlerData.MethodInfo.Invoke(this, new object[] {packetItem.GetData()});
                if (result == null)
                {
                    LogErrorPacket(packetItem, "InvokeRequestAsync, handler method not found");
                    return Task.FromResult((ResponseReply) MakeNoReply());
                }

                return (Task<ResponseReply>) result;
            }
            catch (Exception e)
            {
             
                LogErrorPacket(e, packetItem, "Invoke Exception");
                return null;
            }
        }

        public async Task<bool> InvokeNtfSafeAsync(IPacketItem packetItem)
        {
            try
            {
                await InvokeNtfAsync(packetItem);
                return true;
            }
            catch (Exception e)
            {
                LogErrorPacket(e, packetItem, "InvokeNtfSafeAsync, invoke exception");
                return false;
            }
        }

        private Task InvokeNtfAsync(IPacketItem packetItem)
        {
            var handlerData = PacketHandlerInfoTableMap.Get(SessionService.SessionType, packetItem.GetHeaderData());
            if (handlerData == null)
            {
                LogErrorPacket(packetItem, "InvokeNtfAsync, handler not found");
                return Task.CompletedTask;
            }

            var result = handlerData.MethodInfo.Invoke(this, new object[] { packetItem.GetData() });
            if (result == null)
            {
                LogErrorPacket(packetItem, "InvokeNtfAsync, handler method not found");
                return Task.CompletedTask;
            }

            return (Task)result;
        }

        protected async Task<bool> InvokeNtfSafeAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            try
            {
                await InvokeNtfAsync(handlerData, packetItem);
                return true;
            }
            catch (Exception e)
            {
                LogErrorPacket(e, packetItem, "InvokeNtfSafeAsync, invoke exception");
                return false;
            }
        }

        private Task InvokeNtfAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            var result = handlerData.MethodInfo.Invoke(this, new object[] {packetItem.GetData()});
            if (result == null)
            {
                LogErrorPacket(packetItem, "InvokeNtfAsync, handler method not found");
                return Task.CompletedTask;
            }

            return (Task) result;
        }

        protected void StartReceiveStopwatch()
        {
            _invokeStopwatch.Restart();
        }

        protected void EndReceiveStopwatch(IPacketItem packetItem)
        {
            _invokeStopwatch.Stop();
            if (_invokeStopwatch.Elapsed >= SessionService.Settings.SlowPacketTime)
            {
                AppMetricsEventSource.Instance.SlowProcessing(packetItem);
                LogWarningPacket(packetItem, "LongTime, " + _invokeStopwatch.Elapsed);
            }
        }

        protected virtual void LogPacket(IPacketItem packetItem, string message)
        {
            if (SessionService.Settings.EnableTracePacketLog == false)
                return;

            _logger.LogInformation(
                "Session Message: {Message} | SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), RequestId({RequestId}), Result({Result})",
                message, SessionService.SessionType.Name, SessionId, packetItem.DataType().Name, packetItem.Header(),
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected virtual void LogWarningPacket(IPacketItem packetItem, string message)
        {
            _logger.LogWarning(
                "Session Message: {Message} | SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                message, SessionService.SessionType.Name, SessionId, packetItem.DataType().Name, packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected virtual void LogErrorPacket(IPacketItem packetItem, string message)
        {
            _logger.LogCritical(
                "Session Message: {Message} | SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                message, SessionService.SessionType.Name, SessionId, packetItem.DataType().Name, packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected virtual void LogErrorPacket(Exception e, IPacketItem packetItem, string message)
        {
            _logger.LogCritical(e,
                "Session Message: {Message} | SessionType({SessionType}) SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                message, SessionService.SessionType.Name, SessionId, packetItem?.DataType()?.Name, packetItem?.Header(),
                packetItem?.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem?.GetRequestId(), packetItem?.GetResultCode());
        }
    }
}
