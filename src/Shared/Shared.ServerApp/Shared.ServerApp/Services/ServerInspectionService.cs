using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared.Clock;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerModel;
using Shared.Services.Redis;
using SampleGame.Shared.Common;

namespace Shared.ServerApp.Services
{
    public class ServerInspectionService
    {
        private readonly ILogger<ServerInspectionService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedLockFactory _redLockFactory;
        private readonly UserContextDataService _userCtxService;
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;        

        private ServerInspectionInfo _serverInspectionInfo = new();

        public ServerInspectionService(
            ILogger<ServerInspectionService> logger,
            DatabaseRepositoryService dbRepo,
            RedLockFactory redLockFactory,
            UserContextDataService userCtxService,
            CsvStoreContext csvStoreContext,            
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _redLockFactory = redLockFactory;
            _userCtxService = userCtxService;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;
            
        }


        public async Task InitAsync()
        {
            var appRedis = _redisRepo.GetDb(RedisDatabase.App);
            var inspectionRedis = await appRedis.StringGetAsync(RedisKeys.s_ServerInspection);
            if( inspectionRedis.HasValue == true )
            {
                var inspectionInfo = JsonTextSerializer.Deserialize<ServerInspectionInfo>((string)inspectionRedis);
                SetInspectionInfo(inspectionInfo);
            }
        }

        //public async Task InitAsync()
        //{
        //    using var gateCtx = _dbRepo.GetGateDb();
        //    var serverType = Enum.Parse<ServerLocationType>(_appSettings.Value.AppGroupName);
        //    var serverMaintenance = await gateCtx.ServerMaintenances.FindAsync(serverType);
        //    _serverInspectionInfo = serverMaintenance == null ? new ServerInspectionInfo() : serverMaintenance.ToInspetionInfo();
        //}

        public void SetInspectionInfo(ServerInspectionInfo info )
        {
            _serverInspectionInfo = info;   
        }
        public ServerInspectionInfo InspectionInfo => _serverInspectionInfo;

        public bool CheckNotifyServerInspection()
        {
            if(_serverInspectionInfo.IsInspection == false 
                || _serverInspectionInfo.FromDt == null 
                || _serverInspectionInfo.ToDt == null)
                return false;

            if( _serverInspectionInfo.IsInspection == true )
            {
                if (_serverInspectionInfo.FromDt > AppClock.UtcNow)
                    return true;

            }

            return false;
        }

        public bool IsServerInspectionTime => InspectionInfo.IsInspectionTime();
        public bool CheckInspectionError(string ip)
        {
            // 점검 중 
            if(IsServerInspectionTime)  
            {
                if (!string.IsNullOrEmpty(_serverInspectionInfo.AllowIp) && _serverInspectionInfo.AllowIp.Contains(ip))
                    return false;
                return true;
            }
            return false;
        }
    }
}
