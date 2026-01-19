using System;
using Shared.Gate;
using Shared.Services.Redis;
using FrontEndWeb.Connection.Session;
using FrontEndWeb.Services;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Network.Base;
using Shared.ServerApp.Connection;
using Shared.ServerApp.Services;
using Shared.Session.Base;
using Shared.Session.Services;

namespace FrontEndWeb.Connection.Services
{
    public class ServerSessionService : AppServerSessionServiceBase
    {
        private readonly ILogger<ServerSessionService> _logger;

        public ServerSessionService(
            ILogger<ServerSessionService> logger,
            IServiceProvider serviceProvider,
            FrontEndAppContextService appContext,
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
                    NetServiceType = NetServiceType.Auth,
                    Host = appContext.ExternalHost,
                    Port = appContext.ExternalWebPort,
                },
                new ServerRegisterInfo
                {
                    ProtocolType = ProtocolType.Http,
                    NetServiceType = NetServiceType.Api,
                    Host = appContext.ExternalHost,
                    Port = appContext.ExternalWebPort,
                },
                new ServerRegisterInfo
                {
                    ProtocolType = ProtocolType.Tcp,
                    NetServiceType = NetServiceType.FrontEnd,
                    Host = appContext.ExternalHost,
                    Port = appContext.ExternalFrontendPort,
                }
                //,
                //new ServerRegisterInfo
                //{
                //    ProtocolType = ProtocolType.Tcp,
                //    NetServiceType = NetServiceType.Mail,
                //    Host = appContext.ExternalHost,
                //    Port = appContext.ExternalMailPort,
                //}
            })
        {
            _logger = logger;
        }

        public override SessionBase CreateSession(IConnection connection)
        {
            return new ServerSession(this, ServiceProvider, connection);
        }
    }
}
