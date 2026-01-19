using System;
using System.Threading.Tasks;
using Elastic.Apm;
using Microsoft.Extensions.Logging;
using Shared.Network.Base;
using Shared.Network.Utility;
using Shared.Packet;
using Shared.Session.Extensions;

namespace Shared.Session.Base
{
    public class SessionListenerBase
    {
        private readonly ILogger<SessionListenerBase> _logger;
        private readonly SessionServiceBase _sessionService;

        private readonly TaskWorker<PacketWorkerItem> _packetTaskWorker;

        public SessionListenerBase(
            ILogger<SessionListenerBase> logger,
            SessionServiceBase sessionService
        )
        {
            _logger = logger;
            _sessionService = sessionService;

            _packetTaskWorker = new TaskWorker<PacketWorkerItem>(32, OnReceivedAsync);
        }

        public virtual Task OnStopAsync()
        {
            _packetTaskWorker.Dispose();

            return Task.CompletedTask;
        }

        public virtual void OnConnectionOpened(IConnection connection)
        {
            var session = _sessionService.CreateSession(connection);
            _sessionService.OnSessionOpened(session);
        }

        public virtual void OnConnectionClosed(IConnection connection, int closeReason)
        {
            var session = _sessionService.GetSession(connection.GetId());
            if (session != null)
                _packetTaskWorker.Enqueue(new PacketWorkerItem(connection.GetId(), null));
        }

        public virtual void OnReceived(IConnection connection, object packetItem)
        {
            _packetTaskWorker.Enqueue(new PacketWorkerItem(connection.GetId(), packetItem));
        }

        protected virtual void OnAuthenticated(IConnection connection)
        {
        }
        public virtual void OnSerializeErrored(IConnection connection, SerializeError error)
        {
            var session = _sessionService.GetSession(connection.GetId());
            if (session != null)
            {
                _packetTaskWorker.Enqueue(new PacketWorkerItem(connection.GetId(), null));
            }
            else
            {
                connection.Close();
            }
        }

        public virtual Task OnReceivedAsync(PacketWorkerItem item)
        {
            try
            {
                var session = _sessionService.GetSession(item.Id);
                if (session == null)
                {
                    _logger.LogWarning("SessionListenerBase: Session not found: SessionType({SessionType}), SessionId({SessionId})",
                        _sessionService.SessionType.Name, item.Id);
                    return Task.CompletedTask;
                }

                if (item.Packet == null)
                {
                    if (item.Extra == 0)
                    {
                        if (session.SessionService.IsUserSession)
                        {
                            return CloseUserSessionAsync(session);
                        }
                        else
                        {
                            return _sessionService.OnSessionClosedAsync(session);
                        }
                    }
                    else
                    {
                        return session.KickAsync();
                    }
                }

                return session.OnReceiveAsync(item.Packet as IPacketItem);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "OnReceivedAsync: Exception");
                throw;
            }
        }

        protected async Task CloseUserSessionAsync(SessionBase session)
        {
            await _sessionService.OnSessionClosedAsync(session);
        }
    }
}