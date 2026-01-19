using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Session;
using WebTool.Connection.Services;
using WebTool.Connection.Session;
using WebTool.Services;

namespace WebTool.Connection
{
    public class WebToolOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private readonly WebToolAppContextService _appContext;

        public WebToolOptionsSetup(
            WebToolAppContextService appContext
        )
        {
            _appContext = appContext;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(KestrelServerOptions options)
        {
            // Server
            {
            }

            // WebTool HTTP
            {
                var endPoint = new IPEndPoint(IPAddress.Parse(_appContext.ListenHost), _appContext.ExternalWebPort);
                options.Listen(endPoint);
            }
        }
    }
}
