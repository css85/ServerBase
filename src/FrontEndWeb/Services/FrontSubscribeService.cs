using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared;
using Shared.Clock;
using Shared.CsvData;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using Shared.ServerModel;
using Shared.Services.Redis;
using StackExchange.Redis;
using SampleGame.Shared.Common;

namespace FrontEndWeb.Services
{
    public class FrontSubscribeService
    {
        private readonly ILogger<FrontSubscribeService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedLockFactory _redLockFactory;        
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;                
        private readonly ServerInspectionService _serverInspectionService;
        private readonly PlayerService _playerService;  
        private readonly FrontCheckService _frontCheckService;


        private ISubscriber _subscriber;

        public FrontSubscribeService(
            ILogger<FrontSubscribeService> logger,
            DatabaseRepositoryService dbRepo,
            RedLockFactory redLockFactory,
            CsvStoreContext csvStoreContext,
            RedisRepositoryService redisRepo,
            PlayerService playerService,
            FrontCheckService frontCheckService,
            ServerInspectionService serverInspectionService
            )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _redLockFactory = redLockFactory;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;          
            _serverInspectionService = serverInspectionService;
            _playerService = playerService;
            _frontCheckService = frontCheckService;
        }

        
        
        public async Task OnStartAsync()
        {
            _logger.LogInformation("FrontSubscribeService running.");
                        
            var appRedis = _redisRepo.GetDb(RedisDatabase.App);
            
            _subscriber = appRedis.Multiplexer.GetSubscriber();
            
            await _subscriber.SubscribeAsync(RedisPubSubChannels.RefreshCsv, SubscribeCsv);
            await _subscriber.SubscribeAsync(RedisPubSubChannels.InspectionInfo, SubscribeInspection);            
            await _subscriber.SubscribeAsync(RedisPubSubChannels.RefreshShoppingmallRank, SubscribeShoppingmallRank);
            await _subscriber.SubscribeAsync(RedisPubSubChannels.NewUserConfig, SubscribeNewUserConfig);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {  
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {   
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }

       
        

        private void SubscribeCsv(RedisChannel channel, RedisValue value)
        {
            _logger.LogInformation("SubscribeCsv");

            _csvContext.GetData().LoadCsvDataAll();
        }

        private void SubscribeInspection(RedisChannel channel, RedisValue value)
        {
            _logger.LogInformation("SubscribeInspection : " + value.ToString());
            var info = JsonTextSerializer.Deserialize<ServerInspectionInfo>(value);

            _serverInspectionService.SetInspectionInfo(info);   
        }

        private async void SubscribeShoppingmallRank(RedisChannel channel, RedisValue value)
        {
//            _logger.LogInformation("SubscribeShoppingmallRank");
            await _playerService.SetShoppingmallRankAsync();
        }

        private void SubscribeNewUserConfig(RedisChannel channel, RedisValue value)
        {
            _logger.LogInformation("SubscribeNewUserConfig : " + value.ToString());
            var info = JsonTextSerializer.Deserialize<NewUserConfig>(value);

            _frontCheckService.SetNewUserConfig(info);
        }
    }
}
