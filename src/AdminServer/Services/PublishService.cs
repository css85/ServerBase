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

namespace AdminServer.Services
{
    public class PublishService
    {
        private readonly ILogger<PublishService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;        

        private ISubscriber _subscriber;

        
        public PublishService(
            ILogger<PublishService> logger,
            DatabaseRepositoryService dbRepo,
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _redisRepo = redisRepo;            
        }

        
        public async Task OnStartAsync()
        {
            _logger.LogInformation("PublishService running.");
            using var userCtx = _dbRepo.GetUserDb();            
            var appRedis = _redisRepo.GetDb(RedisDatabase.App); // pub/sub
            
            _subscriber = appRedis.Multiplexer.GetSubscriber();

        }

       
        public async Task PublishCsvAsync()
        {
            await _subscriber.PublishAsync(RedisPubSubChannels.RefreshCsv, 1);
            _logger.LogInformation("PublishCsv");
        }
        
        public async Task PublishTrendInfoAsync(string infoJson)
        {

            await _subscriber.PublishAsync(RedisPubSubChannels.TrendInfo, infoJson);
            _logger.LogInformation("PublishTrendInfo");
        }
        public async Task PublishLikeBestAsync()
        {
            await _subscriber.PublishAsync(RedisPubSubChannels.RefreshLikeBest, 1);
            _logger.LogInformation("PublishLikeBest");
        }

        public async Task PublishShoppingmallRankAsync()
        {
            await _subscriber.PublishAsync(RedisPubSubChannels.RefreshShoppingmallRank, 1);
//            _logger.LogInformation("PublishShoppingmallRank");
        }
    }
}
