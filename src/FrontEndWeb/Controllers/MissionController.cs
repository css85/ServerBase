using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Services.Redis;
using FrontEndWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using StackExchange.Redis;
using Shared.PacketModel;
using Shared.Packet.Models;
using System;
using Shared.ServerApp.Utility;
using Shared.Packet.Server.Extensions;
using SampleGame.Shared.Common;
using System.Numerics;

namespace FrontEndWeb.Controllers
{
    [Route("api/mission")]
    public class MissionController : TokenBasedApiController
    {
        private readonly ILogger<MissionController> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;
        private readonly CsvStoreContext _csvContext;          
        private readonly InventoryService _inventoryService;
        private readonly MissionService _missionService;
        private readonly PlayerService _playerService;
        private readonly ServerInspectionService _serverInspectionService;

        public MissionController(
            ILogger<MissionController> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvContext,
            RedisRepositoryService redisRepo,
            InventoryService inventoryService,
            MissionService missionService,
            PlayerService playerService,
            ServerInspectionService serverInspectionService
        )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvContext;
            _redisRepo = redisRepo;
            _inventoryService = inventoryService;
            _missionService = missionService;
            _playerService = playerService;
            _serverInspectionService = serverInspectionService; 
        }

        [HttpPost("enter-daily")]
        public async Task<ActionResult<EnterDailyMissionRes>> EnterDailyMissionAsync([FromBody] EnterDailyMissionReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<EnterDailyMissionRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();            
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();

            var accountDb = await userCtx.UserAccounts.FindAsync(userSeq);
            var playHour = (AppClock.UtcNow - accountDb.TodayFirstLoginDt).TotalHours;
            if (playHour > 0)
            {
                await _missionService.AddMissionOverwirteAsync(userCtx, storeEventCtx, userSeq, Cond1Type.PlayTime, Cond2Type.Hour, 0, (BigInteger)playHour);
                await userCtx.SaveChangesAsync();
                await storeEventCtx.SaveChangesAsync();
            }   

            var missionInfos = await _missionService.GetDailyMissionInfosAsync(userCtx, userSeq);
            var bonusInfo = await _missionService.GetDailyMissionBonusInfoAsync(userCtx, userSeq);

            return Ok(ResultCode.Success, new EnterDailyMissionRes
            {
                DailyMissionToDtTick = AppClock.UtcNow.AddDays(1).Date.Ticks,
                MissionInfos = missionInfos,
                BonusInfo = bonusInfo
            });
        }

        [HttpPost("enter-mission")]
        public async Task<ActionResult<EnterMissionRes>> EnterMissionAsync([FromBody] EnterMissionReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<EnterMissionRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();
            
            var accountDb = await userCtx.UserAccounts.FindAsync(userSeq);
            var playHour = (AppClock.UtcNow - accountDb.TodayFirstLoginDt).TotalHours;

            if (playHour > 0)
            {
                await _missionService.AddMissionOverwirteAsync(userCtx, storeEventCtx, userSeq, Cond1Type.PlayTime, Cond2Type.Hour, 0, (BigInteger)playHour);
                await userCtx.SaveChangesAsync();
                await storeEventCtx.SaveChangesAsync();
            }

            var dailyInfos = await _missionService.GetDailyMissionInfosAsync(userCtx, userSeq);
            var bonusInfo = await _missionService.GetDailyMissionBonusInfoAsync(userCtx, userSeq);
            var achievInfos = await _missionService.GetAchievMissionInfosAsync(userCtx, userSeq);

            return Ok(ResultCode.Success, new EnterMissionRes
            {
                DailyMissionToDtTick = AppClock.UtcNow.AddDays(1).Date.Ticks,
                DailyInfos = dailyInfos,
                BonusInfo = bonusInfo,
                AchievementInfos = achievInfos,
            });
        }

        [HttpPost("daily-reward")]
        public async Task<ActionResult<RewardDailyMissionRes>> RewardDailyMissionAsync([FromBody] RewardDailyMissionReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<RewardDailyMissionRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();

            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            if( csvData.MissionDailyDicData.TryGetValue(req.MissionIndex, out var missionData) == false )
                return Ok<RewardDailyMissionRes>(ResultCode.NotFound);
            
            if (missionData.MissionDt.Date != AppClock.UtcToday)
                return Ok<RewardDailyMissionRes>(ResultCode.NotMissionTime);

            var missionDb = await userCtx.UserMissions.Where(p => p.UserSeq == userSeq && p.MissionIndex == req.MissionIndex).FirstOrDefaultAsync();
            if( missionDb == null )
                return Ok<RewardDailyMissionRes>(ResultCode.NotMissionSuccess);
            if( (missionData.Cond1 == Cond1Type.Obtain && missionData.Cond2 == Cond2Type.Currency) || 
                (missionData.Cond1 == Cond1Type.Obtain && missionData.Cond2 == Cond2Type.Material) ||
                (missionData.Cond1 == Cond1Type.Consume && missionData.Cond2 == Cond2Type.Currency))
            {

            }
            else
            {
                if (missionDb.MissionCount < missionData.Count)
                    return Ok<RewardDailyMissionRes>(ResultCode.NotMissionSuccess);
            }
            if( missionDb.IsReward == true )
                return Ok<RewardDailyMissionRes>(ResultCode.AlreadyMissionReward);

            var grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);

            var reward = _playerService.GetCalcGoldItemInfo(grade, missionData.RewardInfo);

            var rewardResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"daily_mission", new List<ItemInfo> { reward });
            if (rewardResult.resultCode != ResultCode.Success)
                return Ok<RewardDailyMissionRes>(rewardResult.resultCode);


            missionDb.IsReward = true;
            missionDb.RewardDt = AppClock.UtcNow;
            userCtx.UserMissions.Update(missionDb);

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            var missionInfos = await _missionService.GetDailyMissionInfosAsync(userCtx, userSeq);

            var completeCount = missionInfos.Where(p => p.IsReward).Count();
            // 일일 미션 완료            
            var bonusInfo = new DailyMissionBonusInfo();
            if ( missionInfos.Count == completeCount)
                bonusInfo = await _missionService.SetDailyMissionCompleteAsync(userCtx, userSeq);
            else
                bonusInfo = await _missionService.GetDailyMissionBonusInfoAsync(userCtx, userSeq);

            return Ok(ResultCode.Success, new RewardDailyMissionRes
            {
                MissionIndex = req.MissionIndex,
                MissionInfos = missionInfos,
                RewardsInfo = rewardResult.obtainResult.RewardsInfo,
                RefreshInfo = rewardResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo(),
                BonusInfo = bonusInfo
            }); 
        }

        [HttpPost("achiev-reward")]
        public async Task<ActionResult<RewardAchievementMissionRes>> RewardAchievementMissionAsync([FromBody] RewardAchievementMissionReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<RewardAchievementMissionRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var missionDb = await userCtx.UserAchievements.Where(p => p.UserSeq == userSeq && p.MissionIndex == req.MissionIndex).FirstOrDefaultAsync();
            if (missionDb == null)
                return Ok<RewardAchievementMissionRes>(ResultCode.NotMissionSuccess);

            var maxOrderNum = csvData.MissionAchievementListData.Where(p => p.MissionGroupIndex == req.MissionIndex).Max(p => p.MissionOrder);
            if( missionDb.LastRewardOrderNum == maxOrderNum )
                return Ok<RewardAchievementMissionRes>(ResultCode.InvalidParameter);


            var datas = csvData.MissionAchievementListData.Where(p => p.MissionGroupIndex == req.MissionIndex && p.MissionOrder > missionDb.LastRewardOrderNum).OrderBy(p=>p.MissionOrder).ToList();
            if( datas.Any() == false)
                return Ok<RewardAchievementMissionRes>(ResultCode.NotFound);

            var successOrder = missionDb.LastRewardOrderNum;
            var rewards = new List<ItemInfo>();
            foreach(var data in datas)
            {
                if (data.Count > missionDb.MissionCount)
                    break;

                rewards.Add(data.RewardInfo);
                successOrder = data.MissionOrder;
            }
            if (successOrder == missionDb.LastRewardOrderNum)
                return Ok<RewardAchievementMissionRes>(ResultCode.NotMissionSuccess);

            var grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);
            rewards = _playerService.GetCalcGoldItemInfos(grade, rewards);

            var rewardResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"achievement", rewards);
            if (rewardResult.resultCode != ResultCode.Success)
                return Ok<RewardAchievementMissionRes>(rewardResult.resultCode);

            missionDb.LastRewardOrderNum = successOrder;
            userCtx.UserAchievements.Update(missionDb);
            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new RewardAchievementMissionRes
            {
                AchievementInfo = missionDb.ToAchiementInfo(),
                MissionIndex = req.MissionIndex,
                RewardsInfo = rewardResult.obtainResult.RewardsInfo,
                RefreshInfo = rewardResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo()
            });
        }


    }
}




