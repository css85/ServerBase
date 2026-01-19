using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace WebTool.Connection
{
    public static class WebToolExtensions
    {
        public static IServiceCollection AddWebTool(this IServiceCollection services)
        {
            WebToolOptionsSetup.ConfigureServices(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<KestrelServerOptions>, WebToolOptionsSetup>());

            return services;
        }
    }
}
