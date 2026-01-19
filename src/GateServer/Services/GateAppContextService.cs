using System.Threading.Tasks;
using Common.Config;
using GateServer.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ServerApp.Config;
using Shared.Session.Base;

namespace GateServer.Services
{
    public class GateAppContextService : AppContextServiceBase
    {
        private readonly ILogger<GateAppContextService> _logger;

        public sealed override int AppGroupId { get; protected set; }
        public sealed override string AppGroupName { get; protected set; }
        public sealed override int AppId { get; protected set; }
        public sealed override string AppName { get; protected set; }
        public sealed override string Environment { get; protected set; }
        public sealed override bool IsTestMode { get; protected set; }
        public sealed override bool IsUnitTest { get; protected set; }
        public sealed override string InternalHost { get; protected set; }
        public sealed override int InternalPort { get; protected set; }
        public sealed override bool EnableUnixSocket { get; protected set; }
        public string ExternalHost { get; }
        public int ExternalWebPort { get; }
        public string ListenHost { get; }

        public GateAppContextService(
            ILogger<GateAppContextService> logger,
            IHostEnvironment environment,
            IConfiguration configuration,
            ChangeableSettings<GateAppSettings> appSettings
        ) : base(logger)
        {
            _logger = logger;

            AppGroupId = configuration.GetValue("AppGroupId", appSettings.Value.AppGroupId);
            AppGroupName = configuration.GetValue("AppGroupName", appSettings.Value.AppGroupName);
            AppId = configuration.GetValue("AppId", appSettings.Value.AppId);
            AppName = configuration.GetValue("AppName", appSettings.Value.AppName);
            Environment = environment.EnvironmentName;
            IsTestMode = configuration.GetValue("IsTestMode", appSettings.Value.IsTestMode);
            InternalHost = configuration.GetValue("InternalHost", appSettings.Value.InternalHost);
            InternalPort = configuration.GetValue("InternalPort", appSettings.Value.InternalPort);
            EnableUnixSocket = configuration.GetValue("EnableUnixSocket", appSettings.Value.EnableUnixSocket);

            ExternalHost = configuration.GetValue("ExternalHost", appSettings.Value.ExternalHost);
            ExternalWebPort = configuration.GetValue("ExternalWebPort", appSettings.Value.ExternalWebPort);
            ListenHost = configuration.GetValue("ListenHost", appSettings.Value.ListenHost);
            IsUnitTest = environment.IsEnvironment("UnitTest");
        }

        public override async Task StartAsync()
        {
            using (_logger.BeginScope("GateAppContextService"))
            {
                _logger.LogInformation("----------------GateAppContextService----------------");
                await base.StartAsync();

                _logger.LogInformation("ExternalHost: {ExternalHost}",ExternalHost);
                _logger.LogInformation("ExternalWebPort: {ExternalWebPort}", ExternalWebPort);
                _logger.LogInformation("ListenHost: {ListenHost}", ListenHost);
                _logger.LogInformation("IsUnitTest: {IsUnitTest}", IsUnitTest);
                _logger.LogInformation("--------------------------------------------------");
            }
        }
    }
}
