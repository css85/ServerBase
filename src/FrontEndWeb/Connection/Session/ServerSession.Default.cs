using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using SampleGame.Shared.Utility;
using FrontEndWeb.Connection.Services;
using FrontEndWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.Entities;
using Shared.Entities.Models;

using Shared.Model;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Packet.Models;
using Shared.Packet.Utility;
using Shared.PacketModel;
using Shared.Repository;
using Shared.Repository.Extensions;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.Server.Packet.Internal;
using Shared.ServerApp.Common.Tasks;
using Shared.ServerApp.Config;
using Shared.ServerApp.Connection;
using Shared.ServerApp.Services;
using Shared.Session.Data;
using static Shared.Session.Extensions.ReplyExtensions;

namespace FrontEndWeb.Connection.Session
{
    public partial class ServerSession : AppServerSessionBase
    {
        private new readonly ILogger<ServerSession> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;

        private readonly FrontEndAppContextService _appContext;
        private readonly ChangeableSettings<GameRuleSettings> _gameRule;
        private readonly CsvStoreContext _csvContext;

        private readonly UserFrontendSessionService _userFrontendSessionService;
        private readonly UserContextDataService _userCtxService;
        private readonly SequenceService _seqService;

        public ServerSession(
            AppServerSessionServiceBase appServerSessionService,
            IServiceProvider serviceProvider,
            IConnection connection
        ) : base(appServerSessionService, serviceProvider, connection)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<ServerSession>>();
            _appContext = ServiceProvider.GetRequiredService<FrontEndAppContextService>();
            _gameRule = ServiceProvider.GetRequiredService<ChangeableSettings<GameRuleSettings>>();
            _csvContext = ServiceProvider.GetRequiredService<CsvStoreContext>();
            _dbRepo = ServiceProvider.GetRequiredService<DatabaseRepositoryService>();
            _redisRepo = ServiceProvider.GetRequiredService<RedisRepositoryService>();
            _userFrontendSessionService = ServiceProvider.GetRequiredService<UserFrontendSessionService>();
            _userCtxService = ServiceProvider.GetRequiredService<UserContextDataService>();
            _seqService = ServiceProvider.GetRequiredService<SequenceService>();            
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
    }
}
