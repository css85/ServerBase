using Shared.Repository;

namespace Shared.ServerApp.Config
{
    public class DatabaseSettings
    {
        public DatabaseProviderType DatabaseProviderType { get; set; }

        public string[] GateConnectionStrings { get; set; }
        public string[] UserConnectionStrings { get; set; }
        public string[] StoreEventConnectionStrings { get; set; }
        public string WebToolConnectionString { get; set; }
        public string RedisAdmin { get; set; }
        public string RedisAdminHostAndPort { get; set; }
        public bool EnableQueryLogging { get; set; }
    }
}
