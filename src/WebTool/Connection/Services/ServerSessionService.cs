using System;
using Microsoft.Extensions.Logging;
using Shared.Gate;
using Shared.Network.Base;
using Shared.ServerApp.Connection;
using Shared.ServerApp.Services;
using Shared.Services.Redis;
using Shared.Session.Base;
using Shared.Session.Services;
using StackExchange.Redis.Extensions.Core.Abstractions;
using WebTool.Connection.Session;
using WebTool.Services;

namespace WebTool.Connection.Services
{
    public class ServerSessionService : AppServerSessionServiceBase
    {
        private readonly ILogger<ServerSessionService> _logger;

        public ServerSessionService(
            ILogger<ServerSessionService> logger,
            IServiceProvider serviceProvider,
            WebToolAppContextService appContext,
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
            Array.Empty<ServerRegisterInfo>())
        {
            _logger = logger;
        }

        public override SessionBase CreateSession(IConnection connection)
        {
            return new ServerSession(this, ServiceProvider, connection);
        }
    }
}
