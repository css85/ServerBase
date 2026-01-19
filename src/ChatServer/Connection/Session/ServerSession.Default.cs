using System;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using Microsoft.Extensions.DependencyInjection;
using Shared.Model;
using Shared.Network.Base;
using Shared.Packet.Utility;
using Shared.Server.Packet.Internal;
using Shared.ServerApp.Connection;
using Shared.Session.Data;
using static Shared.Session.Extensions.ReplyExtensions;

namespace ChatServer.Connection.Session
{
    public partial class ServerSession : AppServerSessionBase
    {
        private readonly RedisRepositoryService _redisRepo;

        public ServerSession(
            AppServerSessionServiceBase appServerSessionService,
            IServiceProvider serviceProvider,
            IConnection connection
        ) : base(appServerSessionService, serviceProvider, connection)
        {
            _redisRepo = serviceProvider.GetRequiredService<RedisRepositoryService>();
        }

        public Task InternalForwardNtfAsync(InternalForwardNtf ntf)
        {
            var ntfType = PacketHeaderTable.GetNtfType(ntf.Major, ntf.Minor);

            if (ntf.Body == null)
            {
                return SessionService.SendForwardLocalAsync(ntf.ServiceType, ntf.UserSeq, MakeNtfReply(ntfType));
            }
            var forwardPacketData = JsonTextSerializer.Deserialize(ntf.Body, ntfType);
            var forwardReply = MakeNtfReply((NtfBase)forwardPacketData);

            return SessionService.SendForwardLocalAsync(ntf.ServiceType, ntf.UserSeq, forwardReply);
        }

        public Task<ResponseReply> InternalSendGatewayAsync(InternalGetGatewayEncryptKeyReq _)
        {
            var gatewayKey = _sessionManagementService.GatewayEncryptKey;
     
            return MakeTaskResReply(new InternalGetGatewayEncryptKeyRes
            {
                GatewayEncryptKey = gatewayKey,
            });
        }
    }
}
