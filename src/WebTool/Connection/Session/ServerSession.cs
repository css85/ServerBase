using System;
using Shared.Network.Base;
using Shared.Network.Connection;
using Shared.ServerApp.Connection;
using Shared.Session.Features;

namespace WebTool.Connection.Session
{
    public class ServerSession : AppServerSessionBase
    {
        public ServerSession(
            AppServerSessionServiceBase sessionService,
            IServiceProvider serviceProvider,
            IConnection connection
        ) : base(sessionService, serviceProvider, connection)
        {
        }
    }
}
