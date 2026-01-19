using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Shared.Services.Redis;
using SampleGame.Shared.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using Serilog.Core;
using Shared.Packet.Utility;
using Shared.Repository;
using Shared.Repository.Database;
using Shared.Repository.Extensions;
using Shared.Server.Extensions;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.Session;
using Shared.Session.Serializer;
using Shared.Session.Settings;
using Shared.TcpNetwork.Transport;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using Destructurama;
using MySqlConnector.Logging;
using Shared.Session.Services;
using Trace = System.Diagnostics.Trace;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Shared.ServerApp.Extensions
{
    public static class ServerAppExtensions
    {
        private static readonly string[] _sharedPacketAssemblyNames = { "Shared.Packet.Server" };

        public static Logger CreateServerLogger<T>(this HostBuilderContext con) where T : AppSettings
        {
            var appSettingsSection = con.Configuration.GetSection(typeof(T).Name);
         
            var environment = con.HostingEnvironment.EnvironmentName;
            var appGroupName = appSettingsSection.GetValue(nameof(AppSettings.AppGroupName), "");
            var appName = appSettingsSection.GetValue(nameof(AppSettings.AppName), "");

            var loggerBuilder = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .ReadFrom.Configuration(con.Configuration);

           

            return loggerBuilder.CreateLogger();
        }

        
        
        public static void AddServerApp(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
        {
            PacketHeaderTable.Build(_sharedPacketAssemblyNames);

            var dbSettings = services.AddChangeAbleSettings<DatabaseSettings>(config, nameof(DatabaseSettings));

            if (dbSettings.DatabaseProviderType == DatabaseProviderType.MySQL)
            {
                MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();
            }

            var listener = new SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);

            services.AddLogging();
            services.AddAppClock();

            services.AddChangeAbleSettings<SessionSettings>(config, nameof(SessionSettings));
            services.AddChangeAbleSettings<ServerSessionSettings>(config, nameof(ServerSessionSettings));
            services.AddChangeAbleSettings<GameRuleSettings>(config, nameof(GameRuleSettings));
            services.AddChangeAbleSettings<TokenSettings>(config, nameof(TokenSettings));
            services.AddChangeAbleSettings<ClockSettings>(config, nameof(ClockSettings));
            services.AddChangeAbleSettings<CsvSettings>(config, nameof(CsvSettings));
            services.AddChangeAbleSettings<AppMonitorSettings>(config,nameof(AppMonitorSettings));

            services.AddSingleton<CsvStoreContext>();            
            services.AddSingleton<UserContextDataService>();
            services.AddSingleton<SequenceService>();
            services.AddSingleton<StringFilterService>();
//            services.AddScheduleService<ServerConnectionService>();

            var databaseOptions = new List<DatabaseOption>(); 
            void AddDatabase<TCtx>(string[] dbConnectionStrings)
                where TCtx : PooledDbContext
            {
                if (dbConnectionStrings == null || dbConnectionStrings.Length == 0)
                    return;

                databaseOptions.Add(new DatabaseOption
                {
                    DbContextType = typeof(TCtx),
                    ConnectionStrings = dbConnectionStrings,
                    EnableQueryLogging = dbSettings.EnableQueryLogging,
                });
            }

            var gateDbConnectionStrings = config.GetValue("GateDbConnectionString", dbSettings.GateConnectionStrings);
            AddDatabase<GateCtx>(gateDbConnectionStrings);

            //var exchangeDbConnectionStrings = config.GetValue("ExchangeDbConnectionString", dbSettings.ExchangeConnectionStrings);
            //AddDatabase<ExchangeCtx>(exchangeDbConnectionStrings);

            //var accountDbConnectionStrings = config.GetValue("AccountDbConnectionString", dbSettings.AccountConnectionStrings);
            //AddDatabase<AccountCtx>(accountDbConnectionStrings);

            var userDbConnectionStrings = config.GetValue("UserDbConnectionString", dbSettings.UserConnectionStrings);
            AddDatabase<UserCtx>(userDbConnectionStrings);

            var storeEventDbConnectionStrings = config.GetValue("StoreEventConnectionStrings", dbSettings.StoreEventConnectionStrings);
            AddDatabase<StoreEventCtx>(storeEventDbConnectionStrings);

            //var walletDbConnectionStrings = config.GetValue("WalletDbConnectionString", dbSettings.WalletConnectionStrings);
            //AddDatabase<WalletCtx>(walletDbConnectionStrings);


            var dbRepoOptions = new DatabaseRepositoryServiceOptions
            {
                DatabaseOptions = databaseOptions.ToArray(),
            };
            services.AddDatabaseRepositoryService(dbRepoOptions);

            var settingsSection = config.GetSection(nameof(RedisConfiguration));
            var redisHost = config.GetValue("RedisHost", settingsSection["Hosts:0:Host"]);
            var redisPort = config.GetValue("RedisPort", settingsSection["Hosts:0:Port"]);
            var redisPassword = config.GetValue("RedisPassword", settingsSection["Password"]);

            settingsSection["Hosts:0:Host"] = redisHost;
            settingsSection["Hosts:0:Port"] = redisPort;
            settingsSection["Password"] = redisPassword;
            //            settingsSection["Password"] = config.GetValue("RedisPassword", settingsSection["Password"]);
            var redisConfiguration = settingsSection.Get<RedisConfiguration>();            
            services.AddSingleton(redisConfiguration);
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<RedisRepositoryService>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{redisHost}:{redisPort}";
            });
            services.AddSingleton<IDistributedCache, RedisCache>();

            var endPoints = redisConfiguration.Hosts.Select(h => new RedLockEndPoint(new DnsEndPoint(h.Host, h.Port))).ToList();
            endPoints.ForEach(p => p.Password = redisPassword);
            services.AddSingleton(p=>RedLockFactory.Create(endPoints));

            services.AddSingleton<ITokenService, TokenService>();

            var packetSerializer = new SystemTextJsonPacketSerializer(SystemTextJsonSerializationOptions.Default);
            var encPacketSerializer = new JsonPacketSerializerEncrypt(JsonPacketSerializerEncryptOptions.Default);
            services.AddSingleton(p => packetSerializer);
            services.AddSingleton(p => encPacketSerializer);

            services.AddSingleton(p => new TcpConnectionSettingBase //serverssion - 암호화안됨
            {
                PacketSerializer = packetSerializer,
            });
            services.AddSingleton(p => new TcpEncryptConnectionSettings //userSession - 암호화됨 -> 2 // client-> secretkey -> session-key
            {
                PacketSerializer = encPacketSerializer,
            });

            services.AddSingleton<InitializeService>();

            services.AddSingleton(ArrayPool<byte>.Shared);

            services.AddSessionBasic();
            services.AddSingleton<PacketResolverService>();
            services.AddHostedService<AppMetricsService>();
            services.AddSingleton<MissionService>();
            services.AddSingleton<ServerInspectionService>();
            services.AddSingleton<InventoryService>();
            services.AddSingleton<PlayerService>(); 
            services.AddAppHealthCheck(dbRepoOptions);
        }
        

        public static IServiceCollection AddAppClock(this IServiceCollection services)
        {
            services.AddSingleton<AppClockService>();
            return services;
        }
    }
}