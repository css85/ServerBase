using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Shared.Network.Connection;
using Shared.Session.Features;

namespace Integration.Tests.Services
{
    public class TestServerSessionService
    {
        private readonly ILogger<TestServerSessionService> _logger;

        private readonly List<ServerSessionServiceBase> _serverSessionServices = new();

        private int _connectionId;

        private readonly object _lock = new();

        public TestServerSessionService(
            ILogger<TestServerSessionService> logger)
        {
            _logger = logger;
        }

        public void RegisterServerSessionService(ServerSessionServiceBase serverSessionService)
        {
            lock (_lock)
            {
                foreach (var alreadyServerSessionService in _serverSessionServices)
                {
                    var connectionId = Interlocked.Increment(ref _connectionId);
                    var fromConnection = new LocalConnection(connectionId);
                    var toConnection = new LocalConnection(connectionId);

                    fromConnection.Open(p => alreadyServerSessionService.SessionListener.OnReceived(fromConnection, p));
                    toConnection.Open(p => serverSessionService.SessionListener.OnReceived(toConnection, p));

                    var fromSession = serverSessionService.CreateSession(fromConnection);
                    serverSessionService.PrepareSessionOpen(fromSession);

                    var toSession = alreadyServerSessionService.CreateSession(toConnection);
                    alreadyServerSessionService.PrepareSessionOpen(toSession);

                    serverSessionService.OnSessionOpened(fromSession);
                    alreadyServerSessionService.OnSessionOpened(toSession);
                }

                _serverSessionServices.Add(serverSessionService);
            }
        }
    }
}
