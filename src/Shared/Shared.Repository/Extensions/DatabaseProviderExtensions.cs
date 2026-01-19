using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Shared.Repository.Extensions
{
    public static class DatabaseProviderExtensions
    {
        private static CultureInfo _cultureInfo = new CultureInfo("en-US");

        public static DatabaseProviderType GetDatabaseProviderType(this IConfiguration configuration)
        {
            var configurationValue = configuration.GetSection("DatabaseSettings")["DatabaseProviderType"];
            if (string.IsNullOrEmpty(configurationValue))
                return DatabaseProviderType.None;

            configurationValue = configurationValue.ToLower(_cultureInfo);

            var providerTypes = Enum.GetValues<DatabaseProviderType>();
            foreach (var providerType in providerTypes)
            {
                if (configurationValue == providerType.ToString().ToLower(_cultureInfo))
                {
                    return providerType;
                }
            }

            return DatabaseProviderType.None;
        }

        public static string GetDateTimeDefaultSql(this DatabaseProviderType providerType)
        {
            switch (providerType)
            {
                case DatabaseProviderType.MySQL:
                    return "CURRENT_TIMESTAMP(6)";
                case DatabaseProviderType.LocalDB:
                    return "GETUTCDATE()";
                default:
                    throw new ArgumentOutOfRangeException(nameof(providerType), providerType, null);
            }
        }
    }
}
