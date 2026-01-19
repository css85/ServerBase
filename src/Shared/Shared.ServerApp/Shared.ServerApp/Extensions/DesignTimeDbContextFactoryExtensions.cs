using System;
using System.IO;
using SampleGame.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Repository;
using Shared.ServerApp.Config;

namespace Shared.ServerApp.Extensions
{
    public static class DesignTimeDbContextFactoryExtensions
    {
        public static DbContextOptionsBuilder<T> DbContextOptionsBuilder<T>(string[] args,
            Func<DatabaseSettings, string[]> connectionStringsSelector) where T : DbContext
        {
            var path = Directory.GetCurrentDirectory();

            var environment = args.FindArgValue("--environment");
            var indexString = args.FindArgValue("--index");

            if (int.TryParse(indexString, out var index) == false)
            {
                index = 0;
            }

            var builder =
                new ConfigurationBuilder()
                    .SetBasePath(path)
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{environment}.json", true);

            var config = builder.Build();
            var dbSettingsSection = config.GetSection(nameof(DatabaseSettings));
            var dbSettings = dbSettingsSection.Get<DatabaseSettings>();

            var connectionStrings = connectionStringsSelector(dbSettings);
            var connectionString = connectionStrings[index];

            var optionsBuilder = new DbContextOptionsBuilder<T>();

            switch (dbSettings.DatabaseProviderType)
            {
                case DatabaseProviderType.MySQL:
                {
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                        p => p.MigrationsAssembly("App.Migrations.MySQL"));
                    break;
                }
                case DatabaseProviderType.LocalDB:
                {
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                        p => p.MigrationsAssembly("App.Migrations.LocalDB"));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return optionsBuilder;
        }
    }
}