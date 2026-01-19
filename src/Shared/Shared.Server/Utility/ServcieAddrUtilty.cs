using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Config;

namespace SampleGame.Shared.Utility
{
    public static class ServcieAddrUtilty
    {
        public static T AddChangeAbleSettings<T>(this IServiceCollection services, IConfiguration configuration,
            string name)
            where T : class
        {
            var settingsSection = configuration.GetSection(name);
            services.Configure<T>(settingsSection);
            services.AddSingleton<ChangeableSettings<T>>();
            return settingsSection.Get<T>();
        }

        public static T GetSettings<T>(this IServiceCollection services, IConfiguration configuration, string name)
            where T : class
        {
            var settingsSection = configuration.GetSection(name);
            return settingsSection.Get<T>();
        }
    }
}

