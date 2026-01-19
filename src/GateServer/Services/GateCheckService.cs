using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shared.Services.Redis;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using StackExchange.Redis;
using Shared.Repository.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Collections.Generic;
using Shared.Entities.Models;
using Shared;
using Shared.Packet.Models;
using MaxMind.GeoIP2;

namespace GateServer.Services
{
    public class GateCheckService : IHostedService, IDisposable
    {
        private readonly ILogger<GateCheckService> _logger;
        private readonly DatabaseRepositoryService _dbRepo;        
        private readonly CsvStoreContext _csvStoreContext;        
        private readonly RedisRepositoryService _redisRepo;

        private DatabaseReader _geoIpReader;
        private ISubscriber _subscriber;

        private Timer _timer;
        private Dictionary<Tuple<int, OSType>, ServerLocationType> _serverVersionDic = new(); 
        private Dictionary<ServerLocationType, List<ServiceServerInfo>> _serverInfoDic = new();
        private Dictionary<ServerLocationType, string> _cdnUrlDic = new();
        private Dictionary<ServerLocationType, GateServerMaintenanceModel> _serverMaintenanceDic = new();
        
        public GateCheckService(
            ILogger<GateCheckService> logger,                    
            CsvStoreContext csvStoreContext,
            DatabaseRepositoryService dbRepo,
            RedisRepositoryService redisRepo
        )
        {
            _logger = logger;            
            _csvStoreContext = csvStoreContext;            
            _redisRepo = redisRepo;
            _dbRepo = dbRepo;
        }


        private async void DoWork(object state)
        {
            GetServerInfos();
        }

        public async Task OnStartAsync()
        {
            _logger.LogInformation("GateCheckService running.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            _geoIpReader = new DatabaseReader("GeoLite2-City.mmdb");

            var appRedis = _redisRepo.GetDb(RedisDatabase.App);

            _subscriber = appRedis.Multiplexer.GetSubscriber();
            await _subscriber.SubscribeAsync(RedisPubSubChannels.RefreshCsv, SubscribeCsv);

            GetServerInfos();
        }

        private void SubscribeCsv(RedisChannel channel, RedisValue value)
        {
            _logger.LogInformation("SubscribeCsv");

            _csvStoreContext.GetData().LoadCsvDataAll();
        }

        private void GetServerInfos()
        {
            using var gateCtx = _dbRepo.GetGateDb();
            _serverVersionDic = gateCtx.ServerVersions.ToDictionary(p => new Tuple<int, OSType>(p.ClientVersion, p.OsType), p => p.ServerType);
            _serverMaintenanceDic = gateCtx.ServerMaintenances.ToDictionary(p => p.ServerType, p => p);
            _cdnUrlDic = gateCtx.CdnInfos.ToDictionary(p => p.ServerType, p => p.URL);

            _serverInfoDic = gateCtx.ServerInfos.GroupBy(x => x.ServerType, 
                x=>new ServiceServerInfo { Type = x.NetServiceType, Url = x.URL}, 
                (key, g) => new {type = key, data = g.ToList()})
                .ToDictionary( p=>p.type, p=> p.data);   

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;

        }

        public void Dispose()
        {
            _timer?.Dispose();
        }


        public ServerLocationType GetLocationType(Tuple<int, OSType> clientInfo)
        {
            if (_serverVersionDic.TryGetValue(clientInfo, out var type) == false)
                return ServerLocationType.None;
            return type;
        }

        
        public long CheckServerInspection(string ip, ServerLocationType type)
        {
            if(_serverMaintenanceDic.TryGetValue(type, out var serverMaintenance) == false)
                return 0;

            if (!string.IsNullOrEmpty(serverMaintenance.AllowIpInspection) && serverMaintenance.AllowIpInspection.Contains(ip))
                return 0;

            if( serverMaintenance.IsServerInspection && serverMaintenance.IsInspectionTime(AppClock.UtcNow) )
            {
                return serverMaintenance.InspectionTo.Value.Ticks;
            }

            return 0;

        }

        public string GetCountryCode(string ip)
        {
            var country = "KR";
            try
            {
                var cityData = _geoIpReader.City(ip);
                if (cityData != null)
                {
                    country = cityData.Country.IsoCode;
//                    _logger.LogInformation(string.Format("ip : {0} country : {1} ", ip, country));
                }
            }
            catch (Exception ex)
            {
                return country;
            }
            return country;

        }


        public bool CheckServerAllowCountry(string ip, ServerLocationType type)
        {
            if (_serverMaintenanceDic.TryGetValue(type, out var serverMaintenance) == false)
                return true;

            var country = GetCountryCode(ip);
//            _logger.LogInformation(string.Format("ip : {0} country : {1} ", ip, country));

            if ( !string.IsNullOrEmpty(serverMaintenance.BlockCountry) )
            {
                if(serverMaintenance.BlockCountry.Contains(country))
                {
                    if( string.IsNullOrEmpty( serverMaintenance.AllowIpCountry ) || !serverMaintenance.AllowIpCountry.Contains(ip))
                        return false;
                }
            }
            return true;
        }

        public List<ServiceServerInfo> GetServerInfo(ServerLocationType type)
        {
            if (_serverInfoDic.TryGetValue(type, out var infos) == false)
                return null;
            return infos;
        }

        public string GetCdnInfo(ServerLocationType type)
        {
            if (_cdnUrlDic.TryGetValue(type, out var cdnUrl) == false)
                return string.Empty;
            return cdnUrl;
        }

        

        //public List<ContentConfig> GetContentConfigs(OSType oSType, ServerLocationType serverType)
        //{
        //    var csvData = _csvStoreContext.GetData();
            
        //    var contentConfigs = csvData.ContentCheckListData.Select(x => x.GetContentConfig(oSType, serverType)).ToList();

        //    return contentConfigs;
        //}
    }
}