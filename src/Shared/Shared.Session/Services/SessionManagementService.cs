using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Session.Base;
using Shared.Session.Features;

namespace Shared.Session.Services
{
    public class SessionManagementService
    {
        private readonly ILogger<SessionManagementService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private Dictionary<NetServiceType, ConcurrentDictionary<long, UserSessionBase>> _userSessionMap;

        public string GatewayEncryptKey { get; private set; }

        public void SetGatewayEncryptKey(string key)
        {
            GatewayEncryptKey = key;
        }

        public SessionManagementService(
            ILogger<SessionManagementService>  logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync()
        {
            var sessionServiceArray = _serviceProvider.GetServices<SessionServiceBase>().ToArray();

            _userSessionMap =
                sessionServiceArray.ToDictionary(p => p.ServiceType,
                    p => new ConcurrentDictionary<long, UserSessionBase>());

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        public void AddUserSession(NetServiceType serviceType, long userSeq, UserSessionBase session)
        {
            if (_userSessionMap.TryGetValue(serviceType, out var sessions))
            {
                sessions.AddOrUpdate(userSeq, session, (alreadyUserSeq, alreadySession) => session);
            }
        }

        public void RemoveUserSession(NetServiceType serviceType, long userSeq)
        {
            if (_userSessionMap.TryGetValue(serviceType, out var sessions))
            {
                sessions.TryRemove(userSeq, out _);
            }
        }

        public SessionBase GetSessionByUser(NetServiceType serviceType, long userSeq)
        {
            if (_userSessionMap.TryGetValue(serviceType, out var sessions))
            {
                if (sessions.TryGetValue(userSeq, out var session))
                {
                    return session;
                }
            }

            return null;
        }
        
        public int GetSessionCount(NetServiceType serviceType)
        {
            return _userSessionMap.TryGetValue(serviceType, out var sessionMap) ? sessionMap.Count : 0;
        }
    }
}
