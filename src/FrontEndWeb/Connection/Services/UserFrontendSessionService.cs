using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using FrontEndWeb.Connection.Session;
using FrontEndWeb.Services;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared;
using Shared.Clock;
using Shared.Network.Base;
using Shared.PacketModel;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Session.Features;
using Shared.Session.Services;

namespace FrontEndWeb.Connection.Services
{
    public class UserFrontendSessionService : UserSessionServiceBase
    {
        public readonly ChangeableSettings<GameRuleSettings> GameRule;
        public readonly CsvStoreContext CsvContext;
        public readonly UserContextDataService UserContextDataService;
        public readonly RedLockFactory RedLockFactory;

        public UserFrontendSessionService(
            ILogger<UserFrontendSessionService> logger,
            IServiceProvider serviceProvider,
            ChangeableSettings<GameRuleSettings> gameRule,
            CsvStoreContext csvContext,
            UserContextDataService userContextDataService,
            RedLockFactory redLockFactory
            )
            : base(logger, serviceProvider, NetServiceType.FrontEnd, typeof(UserFrontendSession), typeof(ConnectSessionReq))
        {
            GameRule = gameRule;
            CsvContext = csvContext;
            UserContextDataService = userContextDataService;
            RedLockFactory = redLockFactory;
        }

        public override async Task StartAsync()
        {
            // TODO: 서버 여러대일때 한서버의 데이터만 지우도록
            await RedisRepo.Session.KeyDeleteAsync(RedisKeys.ServiceSessionKey(NetServiceType.FrontEnd));
        }

        public override async Task OnStopAsync()
        {
            // TODO: 서버 여러대일때 한서버의 데이터만 지우도록
            await RedisRepo.Session.KeyDeleteAsync(RedisKeys.ServiceSessionKey(NetServiceType.FrontEnd));
        }

        public override SessionBase CreateSession(IConnection connection)
        {
            return new UserFrontendSession(this, ServiceProvider, connection);
        }

        public new UserFrontendSession GetUserSession(long userSeq)
        {
            return GetUserSession<UserFrontendSession>(userSeq);
        }

        public new IEnumerable<UserFrontendSession> GetAllUserSessions()
        {
            return GetAllUserSessions<UserFrontendSession>();
        }

        public async Task<ResultCode> InitializeSessionAsync(UserFrontendSession session, long userSeq, byte lang, byte osType, string appVersion)
        {
            await RemoveAlreadySessionAsync(userSeq);

            await session.SetUserAuthorizedAsync(userSeq, lang, osType, appVersion);

            return AddUserSession(session);
        }

        public override async Task OnSessionClosedAsync(SessionBase session)
        {
            var userSession = (UserFrontendSession)session;
            if (userSession.UserSeq != 0)
            {
                var now = AppClock.UtcNow;
                using (var userCtx = DbRepo.GetUserDb())
                {
                    var account = userSession.Account;
                    account.LogOutDt = now;
                    userCtx.Update(account);
                    await userCtx.SaveChangesAsync();
                }

                var accountRedis = RedisRepo.GetDb(RedisDatabase.Account);
                var loginTimeTicksValue =
                    await accountRedis.HashGetAsync(string.Format(RedisKeys.hs_UserLoginOut, userSession.UserSeq), "logIn");
                await accountRedis.HashSetAsync(string.Format(RedisKeys.hs_UserLoginOut, userSession.UserSeq),
                    "logOut", now.Ticks);

                var sessionTime = 0;
                if (loginTimeTicksValue.HasValue)
                    sessionTime = (int) TimeSpan.FromTicks(now.Ticks - (long)loginTimeTicksValue).TotalSeconds;

                
            }

            await base.OnSessionClosedAsync(session);
        }

        public void SendNtfAll(NtfReply reply, Predicate<UserFrontendSession> predicate)
        {
            foreach (var session in GetAllUserSessions<UserFrontendSession>())
            {
                if (predicate.Invoke(session))
                    session.SendNtf(reply);
            }
        }

    }
}
