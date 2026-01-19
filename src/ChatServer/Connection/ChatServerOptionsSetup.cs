using System.Net;
using ChatServer.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatServer.Connection
{
    public class ChatServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private readonly ChatAppContextService _appContext;

        public ChatServerOptionsSetup(
            ChatAppContextService appContext
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
