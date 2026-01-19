using System;
using System.Linq;
using System.Threading.Tasks;
using Shared.Gate;
using Shared.Services.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Services;
using Shared.Session.Features;
using Shared.Session.Services;

namespace Shared.ServerApp.Connection
{
    public abstract class AppServerSessionServiceBase : ServerSessionServiceBase
    {
        private readonly ILogger<ServerSessionServiceBase> _logger;

        public readonly CsvStoreContext CsvStoreContext;

        private readonly ServerRegisterInfo[] _serverRegisterInfos;

        

        private bool _isAllServerLoaded = false;

        public AppServerSessionServiceBase(
            ILogger<AppServerSessionServiceBase> logger,
            IServiceProvider serviceProvider,
            RedisRepositoryService redisRepo,
            SessionManagementService sessionManagementService,
            CsvStoreContext csvStoreContext,
            Type sessionType,
            ServerRegisterInfo[] serverRegisterInfos
        )
            : base
            (
                logger,
                serviceProvider,
                redisRepo,
                sessionManagementService,
                sessionType,
                serverRegisterInfos.Select(p => p.NetServiceType).ToArray()
            )
        {
            _logger = logger;
            CsvStoreContext = csvStoreContext;

            _serverRegisterInfos = serverRegisterInfos;
        }

        public bool IsAllServerLoaded()
        {
            return false;
//            return _gateService != null && _isAllServerLoaded;
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();


        }

        public override async Task OnStartedAsync()
        {
            
            _isAllServerLoaded = true;

            

            await base.OnStartedAsync();
        }

        public override async Task OnStopAsync()
        {      
            await base.OnStopAsync();
        }

        //public async Task<bool> ConnectAllServersAsync()
        //{
        //    var isAllConnected = true;
        //    var servers = _gateService.GetServers().ToList();
        //    foreach (var server in servers)
        //    {
        //        if (server.AppGroupId != AppContext.AppGroupId ||
        //            server.AppId != AppContext.AppId)
        //        {
        //            var serverSession = GetServerSession(server.AppId);
        //            if (serverSession != null &&
        //                serverSession.IsConnected)
        //            {
        //                continue;
        //            }

        //            //var connectResult = await ConnectAsync(server);
        //            //if (connectResult != ServerSessionConnectResult.Success)
        //            //{
        //            //    isAllConnected = false;
        //            //}
        //        }
        //    }

        //    return isAllConnected;
        //}
    }
}