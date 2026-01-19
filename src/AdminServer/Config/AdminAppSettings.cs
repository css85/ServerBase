using Shared.Server.Define;
using Shared.ServerApp.Config;

namespace AdminServer.Config
{
    public class AdminAppSettings : AppSettings
    {
        public int ExternalWebPort { get; set; }
//        public ServerServiceType ServerServiceType { get; set; }    // 서버타입 
    }
}
