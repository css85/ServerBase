using System;
using System.Threading.Tasks;
using Shared.Gate;
using Shared.Services.Redis;
using GateServer.Connection.Session;
using GateServer.Services;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Network.Base;
using Shared.Server.Define;
using Shared.Server.Packet.Internal;
using Shared.ServerApp.Connection;
using Shared.ServerApp.Services;
using Shared.Session.Base;
using Shared.Session.Services;
using Shared.Utility;
using static Shared.Session.Extensions.ReplyExtensions;

namespace GateServer.Connection.Services
{
    public class ServerSessionService : AppServerSessionServiceBase
    {
        private readonly ILogger<ServerSessionService> _logger;
        private readonly SessionManagementService _sessionManagementService;
        private readonly RedisRepositoryService _redisRepo;

        public ServerSessionService(
            ILogger<ServerSessionService> logger,
            IServiceProvider serviceProvider,
            GateAppContextService appContext,
            RedisRepositoryService redisRepo,
            SessionManagementService sessionManagementService,
            CsvStoreContext csvStoreContext
            ) : base(
            logger,
            serviceProvider,
            redisRepo,
            sessionManagementService,
            csvStoreContext,
            typeof(ServerSession),
            new[]
            {
                new ServerRegisterInfo
                {
                    ProtocolType = ProtocolType.Http,
                    NetServiceType = NetServiceType.Gate,
                    Host = appContext.ExternalHost,
                    Port = appContext.ExternalWebPort,
                }
            })
        {
            _logger = logger;
            _sessionManagementService = sessionManagementService;
            _redisRepo = redisRepo;
        }

        public override async Task OnStartedAsync()
        {
            var createKey = EncryptProvider.CreateKey();
            _sessionManagementService.SetGatewayEncryptKey(createKey);
            _logger.LogInformation("Generate Key {AvailableServices} in {ServiceType}, {NewKey}",
                string.Join(',', AvailableServiceTypes)
                , ServiceType, createKey);
            await _redisRepo.App.StringSetAsync(RedisKeys.GateWayKey, createKey);

            await base.OnStartedAsync();
            await SendServerAllAsync(MakeNtfReply(new InternalUpdatedGateEncryptKey
            {
                GatewayEncryptKey = createKey,
            }));
        }

        public override SessionBase CreateSession(IConnection connection)
        {
            return new ServerSession(this, ServiceProvider, connection);
        }
    }
}
