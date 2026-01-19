using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Shared;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using Shared.Services.Redis;
using StackExchange.Redis;

namespace AdminServer.Services
{
    public class CommandService
    {
        private readonly ILogger<CommandService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;
        private readonly PublishService _publishService;


        public CommandService(
            ILogger<CommandService> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvStoreContext,
            PublishService publishService,
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;
            _publishService = publishService;
        }

        public async Task ReloadShoppingmallRankAsync()
        {
            using var userCtx = _dbRepo.GetUserDb();
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            var operatingScores = await userCtx.PointHistory
                        .Where(p => p.ItemId == (long)PointType.ShoppingmallManage)
                        .GroupBy(p => p.UserSeq)
                        .Select(p => new { UserSeq = p.Key, MaxQty = p.Max(e => e.AfterQty), MaxDt = p.Max(e => e.RegDt) })
                        .ToListAsync();

            var entries = new List<SortedSetEntry>();
            foreach (var score in operatingScores)
            {
                var datetimeOffset = new DateTimeOffset(score.MaxDt);
                double timestamp = (DateTimeOffset.MaxValue.ToUnixTimeSeconds() - datetimeOffset.ToUnixTimeSeconds()) / 1000000000000d;
                entries.Add(new SortedSetEntry(score.UserSeq, score.MaxQty + timestamp));
            }
            if (entries.Count > 0)
            {
                await redisRank.KeyDeleteAsync(RedisKeys.ss_ShoppingmallRank);
                await redisRank.SortedSetAddAsync(RedisKeys.ss_ShoppingmallRank, entries.ToArray());
            }
        }

        
        
    }
}
