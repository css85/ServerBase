using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AdminServer.Connection
{
    public static class AdminServerExtensions
    {
        public static IServiceCollection AddAdminServer(this IServiceCollection services)
        {
            AdminServerOptionsSetup.ConfigureServices(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<KestrelServerOptions>, AdminServerOptionsSetup>());

            return services;
        }
    }
}
