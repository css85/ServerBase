using System.Net;
using AdminServer.Connection.Services;
using AdminServer.Connection.Session;
using AdminServer.Services;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Session;
using Shared.Session.Services;

namespace AdminServer.Connection
{
    public class AdminServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private readonly AdminAppContextService _appContext;

        public AdminServerOptionsSetup(
            AdminAppContextService appContext
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
//                    options.Listen(IPAddress.Parse("0.0.0.0"), 21028);
                }
        }
    }
}
