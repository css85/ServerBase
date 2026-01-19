using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ChatServer.Connection
{
    public static class ChatServerExtensions
    {
        public static IServiceCollection AddChatServer(this IServiceCollection services)
        {
            ChatServerOptionsSetup.ConfigureServices(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<KestrelServerOptions>, ChatServerOptionsSetup>());

            return services;
        }
    }
}
