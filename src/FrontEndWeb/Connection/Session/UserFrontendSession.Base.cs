using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using Shared.Services.Redis;
using FrontEndWeb.Connection.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Network.Base;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Model;
using Shared.ServerApp.Services;
using Shared.Session.Features;

namespace FrontEndWeb.Connection.Session
{
    public partial class UserFrontendSession : UserSessionBase
    {
        private readonly ILogger<UserFrontendSession> _logger;

        private readonly UserFrontendSessionService _userSessionService;
        private readonly UserContextDataService _userCtxService;
        private readonly ServerSessionService _serverSessionService;
        private readonly CsvStoreContext _csvStoreContext;

        public DatabaseRepositoryService DbRepo => _userSessionService.DbRepo;
        public RedisRepositoryService RedisRepo => _userSessionService.RedisRepo;
        public RedLockFactory RedLockFactory => _userSessionService.RedLockFactory;
        public ChangeableSettings<GameRuleSettings> GameRule => _userSessionService.GameRule;        
        public CsvStoreContext CsvContext => _userSessionService.CsvContext;        
        


        public UserContextData ContextData { get; private set; }
        public byte OsType { get; private set; }
        public string AppVersion { get; private set; }
        public AccountModel Account { get; private set; }        
        public DateTime UserTargetTypeTime { get; private set; }

        public UserFrontendSession(
            UserFrontendSessionService sessionService,
            IServiceProvider serviceProvider,
            IConnection connection)
            : base(sessionService, serviceProvider, connection)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<UserFrontendSession>>();        
            _userSessionService = sessionService;
            _userCtxService = ServiceProvider.GetRequiredService<UserContextDataService>();
            _serverSessionService = ServiceProvider.GetRequiredService<ServerSessionService>();
            _csvStoreContext = ServiceProvider.GetRequiredService<CsvStoreContext>();
        }

        public override async Task OnSessionClosedAsync()
        {
            _userSessionService.UserContextDataService.ReleaseContextData(UserSeq, SessionId);

            await SetLocationAsync(SessionLocationType.None);
            await SetUserAuthTokenAsync(0, 0);

            await base.OnSessionClosedAsync();
        }

        private void SetData(byte osType, string appVersion ,UserContextData contextData)
        {
            OsType = osType;
            AppVersion = appVersion;
            ContextData = contextData;
        }
        
        public async Task SetUserAuthorizedAsync(long userSeq, byte lang, byte osType, string appVersion)
        {
            var UserSeq = userSeq;

            var isAdded = false;
            using (var allUserDbs = _userSessionService.DbRepo.GetAllDb<UserCtx>())
            {
                var userCtx = allUserDbs.Get(UserSeq);

                Account = await userCtx.UserAccounts.FindAsync(UserSeq);


                //var user = await userCtx.Users.FindAsync(UserSeq);
                //if (user == null)
                //{
                //    user = new User
                //    {
                //        Seq = userSeq,
                //        NormalGrade = 1,
                //    };
                //    await userCtx.Users.AddAsync(user);
                //    isAdded = true;
                //}
               
                if (isAdded)
                {
                    await userCtx.SaveChangesAsync();
                }

//                contextData = new UserContextData(wallet, userPoint, userVip, SessionId);
  
            }

//            _userCtxService.SetContextData(contextData);
//            SetData(osType, appVersion, contextData);

            
//            await SetUserAuthTokenAsync(userSeq, lang);
            await SetLocationAsync(SessionLocationType.Frontend);


         
        }

        
    }
}