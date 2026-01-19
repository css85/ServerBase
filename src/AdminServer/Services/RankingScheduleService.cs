using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdminServer.Config;
using Common.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.Services.Redis;
using StackExchange.Redis;

namespace AdminServer.Services
{
    public class RankingScheduleService : IHostedService, IDisposable
    {
        private readonly ILogger<RankingScheduleService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;        
        private readonly PublishService _publishService;
        private readonly PlayerService _playerService;

        private Timer _timer;
        private int _timerCount = 0;
        private DateTime _likeBestDate = DateTime.MinValue.Date;

        public RankingScheduleService(
            ILogger<RankingScheduleService> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvStoreContext,
            RedisRepositoryService redisRepo,
            PublishService publishService,
            PlayerService playerService
            )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;                        
            _publishService = publishService;
            _playerService = playerService;
        }

        
        private async void DoWork(object state)
        {
            _timerCount++;

            await CheckLikeBestInfoAsync();
            if (_timerCount == 60)
            {
                await CheckShoppingmallRankingAsync();
                await CheckCCUAsync();

                await _publishService.PublishShoppingmallRankAsync();
                _timerCount = 0;
            }   
        }
        
        

        public async Task OnStartAsync()
        {
            _logger.LogInformation("RankingScheduleService running.");

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
        public async Task CheckCCUAsync()
        {
            var connectCount = _redisRepo.GetKeysCount(RedisDatabase.User, "UserConnect:*");
//            _logger.LogInformation($"접속자 수 : {connectCount}");

            using var userCtx = _dbRepo.GetUserDb();

            await userCtx.CCUHistory.AddAsync(new CCUHistoryModel
            {
                UserCount = connectCount,
            });
            await userCtx.SaveChangesAsync();
        }
        public async Task CheckShoppingmallRankingAsync()
        {
            if (AppClock.UtcNow.Hour == 0 && AppClock.UtcNow.Minute == 0)
            {
                var csvData = _csvContext.GetData();
                var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
                var redisUser = _redisRepo.GetDb(RedisDatabase.User);
                using var userCtx = _dbRepo.GetUserDb();

                var maxRank = csvData.RankingRewardInfoListData.Max(p => p.RankTo);

                var redisRankingUsers = await redisRank.SortedSetRangeByRankAsync(RedisKeys.ss_ShoppingmallRank, 0, maxRank - 1, Order.Descending);

                var insertMails = new List<UserMailModel>();


                int rank = 1;
                foreach (var user in redisRankingUsers)
                {
                    var rankingData = csvData.RankingRewardInfoListData.Where(p => p.IsValidRank(rank)).FirstOrDefault();
                    if (rankingData != null)
                    {
                        var gradeRedis = await redisUser.HashGetAsync(string.Format(RedisKeys.hs_UserInfo, user), "grade");
                        if (gradeRedis.HasValue == false)
                            continue;
                        var grade = (int)gradeRedis;
                        var rewards = _playerService.GetCalcGoldItemInfos(grade, rankingData.RewardInfos);

                        insertMails.AddRange(rewards.Select(p => new UserMailModel
                        {
                            UserSeq = (long)user,
                            ObtainType = p.ItemType,
                            ObtainId = p.Index,
                            ObtainQty = p.ItemQty,
                            TitleKey = "mail_msg_rankingReward",
                            TitleKeyArg = rank.ToString(),
                            LimitDt = AppClock.UtcNow.AddDays(30),
                        }).ToList());
                    }

                    rank++;
                }
                if (insertMails.Any())
                    await userCtx.UserMails.AddRangeAsync(insertMails);

                await userCtx.SaveChangesAsync();
            }
            else
                return;
            
        }


        private async Task CheckLikeBestInfoAsync()
        {
            if (_likeBestDate.Date != AppClock.UtcNow.Date)
            {
//                await SetLikeBestInfoAsync();
            }
        }
        //public async Task SetLikeBestInfoAsync()
        //{
        //    await _playerService.SetLikeBestInfoAsync(true);
        //    _likeBestDate = AppClock.UtcNow.Date;
        //}
    }
}
