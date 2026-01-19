using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleGame.Shared;
using Shared.Services.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Repository.Services;
using Shared.Session.Base;
using Shared.Session.Services;

namespace Shared.Session.Features
{
    public abstract class UserSessionServiceBase : SessionServiceBase
    {
        private readonly ILogger<UserSessionServiceBase> _logger;

        private readonly SessionManagementService _sessionManagementService;

        public readonly RedisRepositoryService RedisRepo;
        public readonly DatabaseRepositoryService DbRepo;
        public readonly Type SessionEnterPacketType;

        private readonly ConcurrentDictionary<long, UserSessionBase> _seqToSessionMap = new();

        protected UserSessionServiceBase(
            ILogger<UserSessionServiceBase> logger,
            IServiceProvider serviceProvider, 
            NetServiceType netServiceType, 
            Type sessionType,
            Type sessionEnterPacket) 
            : base(logger, serviceProvider, netServiceType, sessionType)
        {
            _logger = logger;
            _sessionManagementService = serviceProvider.GetRequiredService<SessionManagementService>();
            RedisRepo = serviceProvider.GetRequiredService<RedisRepositoryService>();
            DbRepo = serviceProvider.GetRequiredService<DatabaseRepositoryService>();
            SessionEnterPacketType = sessionEnterPacket;
        }

        public override void OnSessionOpened(SessionBase session)
        {
            AppMetricsEventSource.Instance.ConnectionStart(ServiceType,session.SessionId);
            base.OnSessionOpened(session);
        }

        public override Task OnSessionClosedAsync(SessionBase session)
        {
            var userSession = (UserSessionBase)session;
            if (userSession.UserSeq != 0)
            {
                _sessionManagementService.RemoveUserSession(ServiceType, userSession.UserSeq);
                _seqToSessionMap.TryRemove(userSession.UserSeq, out _);
            }

            AppMetricsEventSource.Instance.ConnectionStop(ServiceType,session.SessionId);
            return base.OnSessionClosedAsync(session);
        }

        /// <summary>
        /// UserSeq의 기존 세션을 삭제합니다
        /// </summary>
        /// <param name="userSeq">UserSeq</param>
        /// <returns>기존 세션이 있었는지 여부</returns>
        protected async Task<bool> RemoveAlreadySessionAsync(long userSeq)
        {
            if (_seqToSessionMap.TryGetValue(userSeq, out var alreadySession))
            {
                await alreadySession.KickAsync();

                _seqToSessionMap.TryRemove(userSeq, out _);
                _logger.LogInformation("Remove AlreadySession: {UserSeq} {@AlreadySession}", userSeq, alreadySession);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 현재 SessionService에 세션을 등록합니다
        /// </summary>
        /// <param name="userSession">세션</param>
        /// <returns>기존 세션이 있었는지 여부</returns>
        protected ResultCode AddUserSession(UserSessionBase userSession)
        {
            if (_seqToSessionMap.TryAdd(userSession.UserSeq, userSession) == false)
                return ResultCode.InvalidParameter;

            _sessionManagementService.AddUserSession(ServiceType, userSession.UserSeq, userSession);

            return ResultCode.Success;
        }

        public UserSessionBase GetUserSession(long userSeq)
        {
            return _seqToSessionMap.TryGetValue(userSeq, out var session) ? session : null;
        }

        protected T GetUserSession<T>(long userSeq) where T : UserSessionBase
        {
            return _seqToSessionMap.TryGetValue(userSeq, out var session) ? (T)session : null;
        }

        public IEnumerable<UserSessionBase> GetAllUserSessions()
        {
            foreach (var session in _seqToSessionMap.Values)
            {
                if (session.IsConnected)
                    yield return session;
            }
        }

        protected IEnumerable<T> GetAllUserSessions<T>() where T : UserSessionBase
        {
            foreach (var session in _seqToSessionMap.Values)
            {
                if (session.IsConnected)
                    yield return (T) session;
            }
        }

        public async Task KickAllAsync()
        {
            foreach (var userSession in _seqToSessionMap)
            {
                await userSession.Value.KickAsync();
            }
        }
    }
}