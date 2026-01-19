using Microsoft.Extensions.Configuration;
using Shared.ServerApp.Config;

namespace Shared.ServerApp.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetDbValue(this IConfiguration configuration, string name)
        {
            return configuration?.GetSection(nameof(DatabaseSettings))?[name];
        }
    }
}
