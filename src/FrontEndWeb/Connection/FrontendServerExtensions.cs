using FrontEndWeb.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FrontEndWeb.Connection
{
    public static class FrontendServerExtensions
    {
        public static IServiceCollection AddFrontendServer(this IServiceCollection services)
        {
            FrontendServerOptionsSetup.ConfigureServices(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<KestrelServerOptions>, FrontendServerOptionsSetup>());

            return services;
        }
    }
}
