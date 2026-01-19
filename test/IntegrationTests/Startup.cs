using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Config;
using SampleGame.Shared.Utility;
using Integration.Tests.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Shared.Repository;
using Shared.Repository.Database;
using Shared.Repository.Extensions;
using Shared.Repository.Services;
using Shared.ServerApp.Config;
using Shared.ServerApp.Extensions;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Integration.Tests
{
    public class Startup
    {
        private IConfiguration _configuration;

        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    _configuration = configurationBuilder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("Settings/dbsettings.json", optional: true, reloadOnChange: true)
                        .Build();
                })
                .ConfigureLogging(logger =>
                {
                    logger.ClearProviders();
                })
                .ConfigureServices(services =>
                {
                    var dbSettings =
                        services.AddChangeAbleSettings<DatabaseSettings>(_configuration, nameof(DatabaseSettings));
                    services.AddSingleton<TestServerSessionService>();
                    var databaseOptions = new List<DatabaseOption>();

                    void AddDatabase<TCtx>(string[] dbConnectionStrings)
                        where TCtx : PooledDbContext
                    {
                        databaseOptions.Add(new DatabaseOption
                        {
                            DbContextType = typeof(TCtx),
                            ConnectionStrings = dbConnectionStrings,
                            EnableQueryLogging = dbSettings.EnableQueryLogging,
                        });
                    }

                    AddDatabase<GateCtx>(dbSettings.GateConnectionStrings);
                    AddDatabase<UserCtx>(dbSettings.UserConnectionStrings);
                    AddDatabase<StoreEventCtx>(dbSettings.StoreEventConnectionStrings);
                    
                    services.AddDatabaseRepositoryService(new DatabaseRepositoryServiceOptions
                    {
                        DatabaseOptions = databaseOptions.ToArray(),
                    });
                })
                .UseEnvironment("UnitTest");
        }

        public void Configure(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ITestOutputHelperAccessor accessor,
            IHostApplicationLifetime applicationLifetime)
        {
            loggerFactory.AddSerilog().AddProvider(new XunitTestOutputLoggerProvider(accessor));

            var databaseSettings = serviceProvider.GetRequiredService<ChangeableSettings<DatabaseSettings>>();
            Debug.Assert(_configuration.GetDatabaseProviderType() == DatabaseProviderType.LocalDB);

            var con = ConnectionMultiplexer.Connect(databaseSettings.Value.RedisAdmin);
            con.GetServer(databaseSettings.Value.RedisAdminHostAndPort).FlushAllDatabases();

            var dbRepo = serviceProvider.GetRequiredService<DatabaseRepositoryService>();

#pragma warning disable VSTHRD002
            using (var allGateCtx = dbRepo.GetAllDb<GateCtx>())
            {
                for (var i = 0; i < allGateCtx.Length; i++)
                {
                    var ctx = allGateCtx[i];
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();
                }
            }

            using (var allUserCtx = dbRepo.GetAllDb<UserCtx>())
            {
                for (var i = 0; i < allUserCtx.Length; i++)
                {
                    var ctx = allUserCtx[i];
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();
                }
            }

            using (var allUserCtx = dbRepo.GetAllDb<StoreEventCtx>())
            {
                for (var i = 0; i < allUserCtx.Length; i++)
                {
                    var ctx = allUserCtx[i];
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();
                }
            }

#pragma warning restore VSTHRD002

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                // https://resharper-support.jetbrains.com/hc/en-us/community/posts/360006736719-all-unit-tests-are-finished-but-child-process-is-swamped-by-the-test-runner-process-are-still-running-Terminate-child-process-
                // Don't leave LocalDB process running (fix test runner warning)
                using var process = Process.Start("sqllocaldb", "stop MSSQLLocalDB");
                process?.WaitForExit();

                Process.GetCurrentProcess().Kill();
            });
        }
    }
}