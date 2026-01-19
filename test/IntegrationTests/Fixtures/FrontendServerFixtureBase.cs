using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using FrontEndWeb.Connection.Services;
using FrontEndWeb.Services;
using Integration.Tests.Client;
using Integration.Tests.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Repository.Services;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.Session.Services;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;
using Shared;

namespace Integration.Tests.Fixtures
{
    public abstract class FrontendServerFixtureBase : WebApplicationFactory<FrontEndWeb.Startup>
    {
        private readonly ILogger<FrontendServerFixtureBase> _logger;
        private readonly ITestOutputHelperAccessor _testOutputHelper;
        private readonly TestServerSessionService _testServerSessionService;

        public CsvStoreContext CsvContext { get; private set; }
        public CsvStoreContextData CsvData => CsvContext.GetData();
        public ChangeableSettings<GameRuleSettings> GameRule { get; private set; }
        public ChangeableSettings<TokenSettings> TokenSettings { get; private set; }
        public ServerSessionService ServerSessionService { get; private set; }
        public DatabaseRepositoryService DbRepo { get; private set; }
        
        private int _connectionId;
        public readonly int AppId;

        protected FrontendServerFixtureBase(
            ILogger<FrontendServerFixtureBase> logger,
            ITestOutputHelperAccessor testOutputHelper,
            TestServerSessionService testServerSessionService,
            int appId)
        {
            _logger = logger;
            _testOutputHelper = testOutputHelper;
            _testServerSessionService = testServerSessionService;

            AppId = appId;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();
            builder
                .UseEnvironment("UnitTest")
                .ConfigureAppConfiguration((host, config) =>
                {
                    config.SetBasePath(host.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("appsettings.json")
                        .AddJsonFile("appsettings.UnitTest.json")
                        .AddJsonFile("Settings/dbsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"Settings/dbsettings.UnitTest.json", optional: true, reloadOnChange: true)
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["AppId"] = AppId.ToString(),
                        });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new XunitTestOutputLoggerProvider(_testOutputHelper));
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<ServerSessionListenerService<ServerSessionService>>();
                });

            return builder;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            host.Services.GetRequiredService<ServerSessionListenerService<ServerSessionService>>();

            var serverSessionService = host.Services.GetRequiredService<ServerSessionService>();
            _testServerSessionService.RegisterServerSessionService(serverSessionService);
            ServerSessionService = serverSessionService;

            CsvContext = host.Services.GetRequiredService<CsvStoreContext>();
            GameRule = host.Services.GetRequiredService<ChangeableSettings<GameRuleSettings>>();
            TokenSettings = host.Services.GetRequiredService<ChangeableSettings<TokenSettings>>();
            DbRepo = host.Services.GetRequiredService<DatabaseRepositoryService>();

            return host;
        }

        public async Task<ITestClient> GetTestHttpClientAsync()
        {
            var testHttpClient = new TestHttpClient(CreateClient(),
                new []{NetServiceType.Api,NetServiceType.Auth});
            
            await testHttpClient.InitSession();

            return testHttpClient;
        }

        public async Task<ITestClient> GetTestSessionClientAsync()
        {
            var sessionService = Services.GetRequiredService<UserFrontendSessionService>();

            var client = new TestSessionClient(sessionService, Interlocked.Increment(ref _connectionId));
            await client.InitSession();

            return client;
        }

        
    }
}