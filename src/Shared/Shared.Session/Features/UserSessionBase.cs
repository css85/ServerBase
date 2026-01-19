using System;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Elastic.Apm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.Packet.Models;
using Shared.Server.Define;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Session.Extensions;
using Shared.Session.Serializer;
using Shared.Session.Utility;
using Shared.TcpNetwork.Base;
using Shared.Utility;
using static Shared.Session.Extensions.ReplyExtensions;


namespace Shared.Session.Features
{
    public class UserSessionBase : SessionBase
    {
        private readonly ILogger<UserSessionBase> _logger;
        private readonly UserSessionServiceBase _sessionService;
        private readonly IPacketSerializer _packetSerializer;

        protected UserSessionBase(
            UserSessionServiceBase sessionService,
            IServiceProvider serviceProvider,
            IConnection connection)
            : base(sessionService, serviceProvider, connection)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<UserSessionBase>>();
            _sessionService = sessionService;
            _packetSerializer = ServiceProvider.GetRequiredService<JsonPacketSerializerEncrypt>();
        }

        public string SecretKey => Connection.SecretKey;
        public long UserSeq { get; private set; } = 0;
        public byte Language { get; private set; } = 0;
        public SessionLocation Location { get; protected set; }

        public async Task SetUserAuthTokenAsync(long userSeq, byte lang)
        {
            UserSeq = userSeq;
            Language = lang;
            await _sessionService.RedisRepo.Session.HashSetAsync(
                RedisKeys.ServiceSessionKey(SessionService.ServiceType), UserSeq, SessionService.AppContext.AppId);
        }

        #region SessionLocation

        public async Task<bool> GetLocationAsync()
        {
            var locationValue =
                await _sessionService.RedisRepo.Session.HashGetAsync(RedisKeys.hs_UserSessionLocation, UserSeq);
            if (locationValue.HasValue)
            {
                Location = new SessionLocation(locationValue);
                return true;
            }

            return false;
        }


        public async Task SetLocationAsync(SessionLocationType type, int value = 0, int value2 = 0,
            string valueString = "")
        {
            Location = await SetSessionLocationAsync(UserSeq, type, value, value2, valueString);
        }

        public async Task<SessionLocation> SetSessionLocationAsync(long userSeq,
            SessionLocationType type, int value = 0, int value2 = 0, string valueString = "")
        {
            if (type == SessionLocationType.None)
            {
                await _sessionService.RedisRepo.Session.HashDeleteAsync(RedisKeys.hs_UserSessionLocation,
                    userSeq.ToString());

                return SessionLocation.None;
            }

            var sessionLocation = new SessionLocation
            {
                Type = type,
                Value = value,
                Value2 = value2,
                ValueString = valueString
            };

            await _sessionService.RedisRepo.Session.HashSetAsync(RedisKeys.hs_UserSessionLocation,
                userSeq, sessionLocation.ToString());

            return sessionLocation;
        }

        #endregion

        public override async Task OnReceiveAsync(IPacketItem packetItem)
        {
            Task task;
            switch (packetItem.Type())
            {
                case PacketType.Request:
                    task = OnRequestReceiveAsync(packetItem);
                    break;
                default:
                    LogErrorPacket(packetItem, "OnReceive, invalid packet type");
                    task = Task.CompletedTask;
                    break;
            }

            await task.ConfigureAwait(false);
        }

        protected override Task<ResponseReply> OnCheckPacketAsync(PacketHandlerData handlerData, IPacketItem packetItem)
        {
            if (_sessionService.SessionEnterPacketType == packetItem.DataType())
            {
            }
            else if (UserSeq == 0)
            {
                return Task.FromResult(MakeResReply(handlerData.ReturnType, ResultCode.NotAuthorized));
            }

            return base.OnCheckPacketAsync(handlerData, packetItem);
        }
        protected override void SendPacket(IPacketItem packetItem)
        {
            var dataBytes = _packetSerializer.Serialize(packetItem);
            var secretKey = packetItem.Type() switch
            {
                PacketType.Ntf => _sessionManagementService.GatewayEncryptKey,
                _=>Connection.IsAuthenticated ? Connection.SecretKey : _sessionManagementService.GatewayEncryptKey
            };

            _logger.LogTrace(">>|{SessionId}|{PacketType}|{DataType}|{SecretKey}|{IsAuthorized}",
                SessionId,
                packetItem.Type(),
                packetItem.DataType().Name,
                secretKey,
                Connection.IsAuthenticated ? null : "Connected");

            byte[] bytes = Array.Empty<byte>();
            try
            {
                bytes = _sessionService.AppContext.IsUnitTest==false ? EncryptProvider.EncryptAes256(secretKey, dataBytes) : dataBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{SessionId} | Encrypt Fail {SecretKey}", SessionId, secretKey);
            }
            
            packetItem.SetDataBytes(bytes);

            base.SendPacket(packetItem);
        }

        protected override void LogPacket(IPacketItem packetItem, string message)
        {
            if (SessionService.Settings.EnableTracePacketLog == false)
                return;

            _logger.LogInformation(
                "Type({SessionType}) | {Message} | UserSeq({UserSeq})  SessionId({SessionId}) DataType({DataType}), Header({Header}), RequestId({RequestId}), Result({Result})",
                SessionService.SessionType.Name, message, UserSeq, SessionId, packetItem.DataType().Name,
                packetItem.Header(),
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected override void LogWarningPacket(IPacketItem packetItem, string message)
        {
            _logger.LogWarning(
                "Type({SessionType}) | {Message} | UserSeq({UserSeq})  SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                SessionService.SessionType.Name, message, UserSeq, SessionId, packetItem.DataType().Name,
                packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected override void LogErrorPacket(IPacketItem packetItem, string message)
        {
            _logger.LogCritical(
                "Type({SessionType}) | {Message} | UserSeq({UserSeq})  SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                SessionService.SessionType.Name, message, UserSeq, SessionId, packetItem.DataType().Name,
                packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }

        protected override void LogErrorPacket(Exception e, IPacketItem packetItem, string message)
        {
            _logger.LogCritical(e,
                "Type({SessionType}) | {Message} | UserSeq({UserSeq})  SessionId({SessionId}) DataType({DataType}), Header({Header}), Data({Data}), RequestId({RequestId}), Result({Result})",
                SessionService.SessionType.Name, message, UserSeq, SessionId, packetItem.DataType().Name,
                packetItem.Header(),
                packetItem.GetData() != null ? JsonTextSerializer.Serialize(packetItem.GetData(), packetItem.DataType()) : "null",
                packetItem.GetRequestId(), packetItem.GetResultCode());
        }
    }
}