using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Network.Base;
using Shared.Server.Define;
using Shared.Server.Packet.Internal;
using Shared.ServerApp.Services;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Session.Features;
using Shared.Session.PacketModel;
using static Shared.Session.Extensions.ReplyExtensions;

namespace Shared.ServerApp.Connection
{
    public class AppServerSessionBase : ServerSessionBase
    {
        protected readonly ILogger<AppServerSessionBase> _logger;
        private readonly RedisRepositoryService _redisRepo;

        private readonly BaseConsoleCommandService _commandService;

        protected new readonly AppServerSessionServiceBase SessionService;
        private readonly AppContextServiceBase _appContext;

        protected AppServerSessionBase(
            AppServerSessionServiceBase sessionService,
            IServiceProvider serviceProvider,
            IConnection connection
        )
            : base(
                sessionService,
                serviceProvider,
                connection
            )
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<AppServerSessionBase>>();
            _redisRepo = ServiceProvider.GetRequiredService<RedisRepositoryService>();

            SessionService = sessionService;

            _commandService = serviceProvider.GetService<BaseConsoleCommandService>();
            _appContext = SessionService.AppContext;
        }

       
        private async Task SynchronizedGatewayKey()
        {
            if(SessionService.ServiceType == NetServiceType.Internal)
            {
                var result = await SessionService.SendAsync<InternalGetGatewayEncryptKeyRes>(NetServiceType.Gate,
                    MakeReqReply(new InternalGetGatewayEncryptKeyReq()));
                if (result == null)
                {
                    var gateKey = await _redisRepo.App.StringGetAsync(RedisKeys.GateWayKey);
                    if (gateKey.HasValue)
                    {
                        _sessionManagementService.SetGatewayEncryptKey(gateKey);
                    }
                    else
                    {
                        _logger.LogError("Connect to Gateway Server failed!!!");
                    }
                    return;
                }
                
                if (result.Result == ResultCode.Success)
                {
                    _sessionManagementService.SetGatewayEncryptKey(result.Data.GatewayEncryptKey);
                }
            }
        }

      
        public async Task InternalRefreshServerAsync(InternalRefreshServerNtf ntf)
        {
         //   await _gateService.RefreshAsync();
        }
     
        public async Task InternalReceiveGatewayKeyAsync(InternalUpdatedGateEncryptKey ntf)
        {
            if (_appContext.IsUnitTest)
                return;
            
            var gatewayKey = _sessionManagementService.GatewayEncryptKey;
            var dbKey = await _redisRepo.App.StringGetAsync(RedisKeys.GateWayKey);
            _logger.LogTrace("Updated GatewayKey in Redis {SessionType}, {OldKey} | {NewKey} |{DbKey}",
                SessionService.SessionType.Name, gatewayKey, ntf.GatewayEncryptKey, dbKey);
           _sessionManagementService.SetGatewayEncryptKey(ntf.GatewayEncryptKey);
        }
    }
}