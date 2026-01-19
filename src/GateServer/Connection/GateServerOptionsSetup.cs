using System.Net;
using GateServer.Connection.Services;
using GateServer.Connection.Session;
using GateServer.Services;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Session;
using Shared.Session.Services;

namespace GateServer.Connection
{
    public class GateServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private readonly GateAppContextService _appContext;

        public GateServerOptionsSetup(
            GateAppContextService appContext
        )
        {
            _appContext = appContext;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(KestrelServerOptions options)
        {
                if(_appContext.EnableUnixSocket)
                {
                }
                else
                {
                    //Web
                    options.Listen(IPAddress.Parse(_appContext.ListenHost), _appContext.ExternalWebPort);
                }
        }
    }
}
