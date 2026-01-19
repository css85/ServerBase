using System;
using System.Threading;
using System.Threading.Tasks;
using AdminServer.Config;
using Common.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Packet.Models;
using Shared.Packet.Server.Extensions;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerModel;
using Shared.Services.Redis;
using StackExchange.Redis;
using SampleGame.Shared.Common;

namespace AdminServer.Services
{
    public class AdminScheduleService : IHostedService, IDisposable
    {
        private readonly ILogger<AdminScheduleService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;                
        private readonly ChangeableSettings<AdminAppSettings> _appSettings;

        private ISubscriber _subscriber;

        private Timer _timer;        
        private ServerInspectionInfo _serverInspectionInfo = new();

        
        public AdminScheduleService(
            ILogger<AdminScheduleService> logger,
            DatabaseRepositoryService dbRepo,
            RedisRepositoryService redisRepo,
            ChangeableSettings<AdminAppSettings> appSettings,
            IDistributedCache cache)
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _redisRepo = redisRepo;            
            _appSettings = appSettings;
        }

        private async void DoWork(object state)
        {   
            await CheckServerInspectionAsync(); // 점검 체크
        }
        
        

        public async Task OnStartAsync()
        {
            _logger.LogInformation("AdminScheduleService running.");
            
            var pveRedis = _redisRepo.GetDb(RedisDatabase.Ingame);
            var appRedis = _redisRepo.GetDb(RedisDatabase.App); // pub/sub
            _subscriber = appRedis.Multiplexer.GetSubscriber();

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
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

        

        public async Task CheckServerInspectionAsync()
        {
            using var gateCtx = _dbRepo.GetGateDb();
            var serverType = Enum.Parse<ServerLocationType>(_appSettings.Value.AppGroupName);
            var serverMaintenance = await gateCtx.ServerMaintenances.FindAsync(serverType);
            if (serverMaintenance == null)
                return;

            var inspectionInfo = serverMaintenance.ToInspetionInfo();
            
            var appRedis = _redisRepo.GetDb(RedisDatabase.App);
            await appRedis.StringSetAsync(RedisKeys.s_ServerInspection, JsonTextSerializer.Serialize(inspectionInfo));

            bool isChange = false;

            if (serverMaintenance.IsServerInspection != _serverInspectionInfo.IsInspection)
                isChange = true;
            else if ( serverMaintenance.InspectionFrom != _serverInspectionInfo.FromDt)
                isChange = true;
            else if (serverMaintenance.InspectionTo != _serverInspectionInfo.ToDt)
                isChange = true;
            else if(serverMaintenance.AllowIpInspection != _serverInspectionInfo.AllowIp)
                isChange = true;


            if (isChange)
            {
                _serverInspectionInfo = inspectionInfo;
                await _subscriber.PublishAsync(RedisPubSubChannels.InspectionInfo, JsonTextSerializer.Serialize(_serverInspectionInfo));
            }
        }
        
    }
}
