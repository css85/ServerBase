using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace GateServer.Connection
{
    public static class GateServerExtensions
    {
        public static IServiceCollection AddGateServer(this IServiceCollection services)
        {
            GateServerOptionsSetup.ConfigureServices(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<KestrelServerOptions>, GateServerOptionsSetup>());

            return services;
        }
    }
}
