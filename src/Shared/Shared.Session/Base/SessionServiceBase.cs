using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Network.Base;
using Shared.Network.Utility;
using Shared.Packet;
using Shared.Session.Features;
using Shared.Session.Settings;

namespace Shared.Session.Base
{
    public abstract class SessionServiceBase
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly NetServiceType ServiceType;
        public readonly string ServiceTypeString;
        public readonly Type SessionType;
        public readonly string SessionTypeString;
        public readonly AppContextServiceBase AppContext;
        public readonly bool IsUserSession;

        public SessionSettings Settings => _sessionSettings.Value;

        private readonly ILogger<SessionServiceBase> _logger;
        private readonly ChangeableSettings<SessionSettings> _sessionSettings;
        private readonly ConcurrentDictionary<int, SessionBase> _idToSessionMap = new();

        private int _lastSessionId;
        private readonly object _sessionIdLock = new();

        protected SessionServiceBase(
            ILogger<SessionServiceBase> logger,
            IServiceProvider serviceProvider,
            NetServiceType netServiceType,
            Type sessionType)
        {
            _logger = logger;
            ServiceProvider = serviceProvider;
            ServiceType = netServiceType;
            ServiceTypeString = ServiceType.ToString();
            SessionType = sessionType;
            SessionTypeString = SessionType.Name;

            IsUserSession = sessionType.IsAssignableTo(typeof(UserSessionBase));

            AppContext = serviceProvider.GetRequiredService<AppContextServiceBase>();
            _sessionSettings = ServiceProvider.GetRequiredService<ChangeableSettings<SessionSettings>>();
        }

        public int GetSessionId()
        {
            lock (_sessionIdLock)
            {
                _lastSessionId++;
                if (_lastSessionId == int.MaxValue)
                    _lastSessionId = 1;

                return _lastSessionId;
            }
        }

        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnStartedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnStopAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnReceivedAsync(PacketWorkerItem item)
        {
            if (_idToSessionMap.TryGetValue(item.Id, out var session) == false)
            {
                _logger.LogWarning("SessionServiceBase: Session not found: SessionType({SessionType}), SessionId({SessionId})",
                    SessionType.Name, item.Id);
                return Task.CompletedTask;
            }

            var packetItem = item.Packet as IPacketItem;
            return session.OnReceiveAsync(packetItem);
        }

        public int GetSessionCount()
        {
            return _idToSessionMap.Count;
        }

        public IEnumerable<SessionBase> GetAllSessions()
        {
            return _idToSessionMap.Values.Where(p => p.IsConnected);
        }

        public abstract SessionBase CreateSession(IConnection connection);

        public void PrepareSessionOpen(SessionBase session)
        {
            _idToSessionMap.TryAdd(session.SessionId, session);
        }
        
        public virtual void OnSessionOpened(SessionBase session)
        {
            _idToSessionMap.TryAdd(session.SessionId, session);

            _logger.LogDebug("SessionOpened | {ServiceType} | SessionId: {SessionId}", ServiceType, session.SessionId);

            session.OnSessionOpened();
        }

        public virtual Task OnSessionClosedAsync(SessionBase session)
        {
            _idToSessionMap.TryRemove(session.SessionId, out _);

            _logger.LogDebug("SessionClosed | {ServiceType} | SessionId: {SessionId}", ServiceType, session.SessionId);

            return session.OnSessionClosedAsync();
        }

        public SessionBase GetSession(int sessionId)
        {
            return _idToSessionMap.TryGetValue(sessionId, out var session) ? session : null;
        }

        public async Task CheckTimeoutAsync()
        {
            var pingTime = DateTime.UtcNow - _sessionSettings.Value.PingRequiredTime;
            var kickTime = DateTime.UtcNow - _sessionSettings.Value.PingTimeout;

            foreach (var session in _idToSessionMap.Values)
            {
                if (session.IsConnected)
                {
                    if (session.LastReceivedTime < kickTime)
                    {
                        await session.KickAsync();
                    }
                    else if (session.LastReceivedTime < pingTime)
                    {
                        if ((session.Flags & SessionFlags.IsPing) == 0)
                        {
                            session.SendPing();
                        }
                    }
                }
            }
        }
    }
}
