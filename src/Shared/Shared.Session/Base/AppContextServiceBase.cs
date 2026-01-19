using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Shared.Session.Base
{
    public abstract class AppContextServiceBase
    {
        private readonly ILogger<AppContextServiceBase> _logger;

        public abstract int AppGroupId { get; protected set; }
        public abstract string AppGroupName { get; protected set; }
        public abstract int AppId { get; protected set; }
        public abstract string AppName { get; protected set; }
        public abstract string Environment { get; protected set; }
        public abstract bool IsTestMode { get; protected set;}
        public abstract bool IsUnitTest { get; protected set;}
        public abstract string InternalHost { get; protected set; }
        public abstract int InternalPort { get; protected set; }
        public abstract bool EnableUnixSocket { get; protected set;}

        protected AppContextServiceBase(
            ILogger<AppContextServiceBase> logger
        )
        {
            _logger = logger;
        }

        public virtual Task StartAsync()
        {
            _logger.LogInformation("AppGroupId: {AppGroupId}", AppGroupId);
            _logger.LogInformation("AppGroupName: {AppGroupName}", AppGroupName);
            _logger.LogInformation("AppId: {AppId}", AppId);
            _logger.LogInformation("AppName: {AppName}", AppName);
            _logger.LogInformation("Environment: {Environment}", Environment);
            _logger.LogInformation("IsTestMode: {IsTestMode}", IsTestMode);
            _logger.LogInformation("IsUnitTest: {IsUnitTest}", IsUnitTest);
            _logger.LogInformation("InternalHost: {InternalHost}", InternalHost);
            _logger.LogInformation("InternalPort: {InternalPort}", InternalPort);
            _logger.LogInformation("ListenInternalPort: {InternalPort}", InternalPort);
            _logger.LogInformation("EnableUnixSocket: {EnableUnixSocket}", EnableUnixSocket);            

            return Task.CompletedTask;
        }
    }
}
