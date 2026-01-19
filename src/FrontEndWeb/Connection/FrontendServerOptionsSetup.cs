using System.Net;
using FrontEndWeb.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FrontEndWeb.Connection
{
    public class FrontendServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private readonly FrontEndAppContextService _appContext;

        public FrontendServerOptionsSetup(
            FrontEndAppContextService appContext
        )
        {
            _appContext = appContext;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(KestrelServerOptions options)
        {
            if (_appContext.EnableUnixSocket)
            {
                //Web
                options.ListenUnixSocket($"/var/run/tigerclaw/{_appContext.AppId}_web.sock");

            }
            else
            {
                //Web
                options.Listen(IPAddress.Parse(_appContext.ListenHost), _appContext.ExternalWebPort);                
            }

        }
    }
}

