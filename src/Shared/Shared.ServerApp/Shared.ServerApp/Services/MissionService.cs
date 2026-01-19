using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using RedLockNet.SERedis;
using Shared;
using Shared.Clock;
using Shared.CsvData;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.Server.Packet.Internal;
using Shared.ServerModel;
using Shared.Services.Redis;
using StackExchange.Redis;
using Shared.Packet.Server.Extensions;
using SampleGame.Shared.Common;
using Elasticsearch.Net.Specification.IndicesApi;
using System.Runtime.InteropServices.Marshalling;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Numerics;

namespace Shared.ServerApp.Services
{
    public class MissionService
    {
        private readonly ILogger<MissionService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedLockFactory _redLockFactory;
        private readonly UserContextDataService _userCtxService;
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;

        public MissionService(
            ILogger<MissionService> logger,
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
        
        public async Task<List<MissionInfo>> GetDailyMissionInfosAsync(UserCtx userCtx, long userSeq)
        {
            var csvData = _csvContext.GetData();
            var dailyMissionDatas = csvData.MissionDailyDicData.Values.Where(p => p.MissionDt.Date == AppClock.UtcNow.Date).ToList();
            var dailyIndexList = dailyMissionDatas.Select(p => p.Index).ToList();

            var missionDbs = await userCtx.UserMissions.Where(p => p.UserSeq == userSeq && dailyIndexList.Contains(p.MissionIndex)).ToListAsync();

            var missionInfos = new List<MissionInfo>();
            foreach (var mission in dailyMissionDatas)
            {
                var db = missionDbs.Where(p => p.MissionIndex == mission.Index).FirstOrDefault();
                var count = db == null ? 0 : db.MissionCount;
                var isReward = db == null ? false : db.IsReward;

                missionInfos.Add(new MissionInfo
                {
                    MissionIndex = mission.Index,
                    CurrentCount = count,
                    MaxCount = mission.Count,
                    IsReward = isReward,
                    ToDtTick = mission.MissionDt.AddDays(1).Ticks,
                });
            }
            return missionInfos;
        }
        public async Task<List<AchievementInfo>> GetAchievMissionInfosAsync(UserCtx userCtx, long userSeq)
        {
            var csvData = _csvContext.GetData();
            var infos = new List<AchievementInfo>();
            var achievGroupIndexList = csvData.MissionAchievementListData.Select(x => x.MissionGroupIndex).Distinct().ToList();
            if (achievGroupIndexList.Count == 0)
                return infos;

            var achievInfoDbInfos = await userCtx.UserAchievements.Where(p => p.UserSeq == userSeq).Select(p => p.ToAchiementInfo()).ToListAsync();
            if(achievInfoDbInfos.Any())
            {
                infos.AddRange(achievInfoDbInfos);

                var dbIndexList = achievInfoDbInfos.Select(p => p.MissionIndex).ToList();
                infos.AddRange( achievGroupIndexList.Where(p => dbIndexList.Contains(p) == false).Select(p => new AchievementInfo(p)).ToList());
            }
            else
            {
                infos.AddRange(achievGroupIndexList.Select(p => new AchievementInfo(p)).ToList());
            }
            return infos;

        }

        public async Task<DailyMissionBonusInfo> GetDailyMissionBonusInfoAsync(UserCtx userCtx, long userSeq)
        {
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var twodayData = csvData.MissionTwoDayBonusListData.Where(p => p.MissionDt.Date == AppClock.UtcNow.Date).FirstOrDefault();
            var redisKey = string.Format(RedisKeys.hs_DailyMissionComplete, userSeq);
            var bonusRedis = await redisUser.HashGetAllAsync(redisKey);

            if( bonusRedis.Any() == false )
                return new DailyMissionBonusInfo(twodayData.RewardInfo);

            var dataDic = bonusRedis
               .Select(e => new { key = (string)e.Name, Value = e.Value })
               .ToDictionary(e => e.key, e => e.Value);

            var completeCount = (int)dataDic["completeCount"];
            var lastDate = DateTime.Parse((string)dataDic["lastDate"]);
            var isRewarded = (bool)dataDic["isRewarded"];
            var diffDate = AppClock.UtcNow.Date - lastDate.Date;
            if( diffDate.TotalDays >= 2 )
                return new DailyMissionBonusInfo(twodayData.RewardInfo);
            else if( diffDate.TotalDays == 1 ) 
            {
                if(completeCount >= 2)
                    return new DailyMissionBonusInfo(twodayData.RewardInfo);
                else
                    return new DailyMissionBonusInfo(1, twodayData.RewardInfo);
            }
            else
            {
                return new DailyMissionBonusInfo(completeCount, twodayData.RewardInfo, isRewarded);
            }
        }

        public async Task<DailyMissionBonusInfo> SetDailyMissionCompleteAsync(UserCtx userCtx, long userSeq)
        {
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var twodayData = csvData.MissionTwoDayBonusListData.Where(p => p.MissionDt.Date == AppClock.UtcNow.Date).FirstOrDefault();

            var dailyCompleteDb = new UserDailyMissionCompleteModel
            {
                UserSeq = userSeq,
                Date = AppClock.UtcNow.Date,
            };
            await userCtx.UserDailyMissionCompletes.AddAsync(dailyCompleteDb);
            await userCtx.SaveChangesAsync();

            var redisKey = string.Format(RedisKeys.hs_DailyMissionComplete, userSeq);
            var bonusRedis = await redisUser.HashGetAllAsync(redisKey);

            if (bonusRedis.Any() == false)
            {
                await redisUser.HashSetAsync(redisKey, new[]
                {
                    new HashEntry("completeCount", 1),
                    new HashEntry("lastDate", AppClock.UtcNow.ToShortDateString()),
                    new HashEntry("isRewarded", false)
                });

                return new DailyMissionBonusInfo(1, twodayData.RewardInfo);
            }
                
            var dataDic = bonusRedis
               .Select(e => new { key = (string)e.Name, Value = e.Value })
               .ToDictionary(e => e.key, e => e.Value);

            var completeCountRedis = (int)dataDic["completeCount"];
            var lastDateRedis = DateTime.Parse((string)dataDic["lastDate"]);
            var isRewardedRedis = (bool)dataDic["isRewarded"];
            var diffDate = AppClock.UtcNow.Date - lastDateRedis.Date;


            int setCompleteCount = 1;
            bool setIsRewarded = false;
            string setLastDate = AppClock.UtcNow.ToShortDateString();

            var bonusInfo = new DailyMissionBonusInfo(1, twodayData.RewardInfo); 

            if( diffDate.TotalDays == 1)
            {
                if( completeCountRedis < 2)
                {
                    setCompleteCount = completeCountRedis + 1;
                }
            }
            else if( diffDate.TotalDays < 1)
            {
                return new DailyMissionBonusInfo(completeCountRedis, twodayData.RewardInfo, isRewardedRedis);
            }


            await redisUser.HashSetAsync(redisKey, new[]
            {
                new HashEntry("completeCount", setCompleteCount),
                new HashEntry("lastDate", AppClock.UtcNow.ToShortDateString()),
                new HashEntry("isRewarded", setIsRewarded)
            });

            return new DailyMissionBonusInfo(setCompleteCount, twodayData.RewardInfo, setIsRewarded);
        }

        private async Task UpdateDailyMissionDbCountUpAsync(UserCtx userCtx, CsvStoreContextData csvData, long userSeq, List<CountUpMission> addMissionList)
        {
            var indexList = addMissionList.Select(p => p.MissionIndex).ToList();
            var userMissions = await userCtx.UserMissions.Where(p => p.UserSeq == userSeq && indexList.Contains(p.MissionIndex)).ToListAsync();

            var notExistMissions = new List<UserMissionModel>();

            foreach (var mission in addMissionList)
            {
                var missionDb = userMissions.Where(p => p.MissionIndex == mission.MissionIndex).FirstOrDefault();
                if (missionDb == null)
                {
                    notExistMissions.Add(new UserMissionModel
                    {
                        UserSeq = userSeq,
                        MissionIndex = mission.MissionIndex,
                        MissionCount = mission.Count,
                        IsReward = false,
                    });
                }
                else
                {
                    if (missionDb.IsReward || missionDb.MissionCount >= csvData.MissionDailyDicData[missionDb.MissionIndex].Count)
                        continue;

                    missionDb.MissionCount += mission.Count;
                    userCtx.UserMissions.Update(missionDb);
                }
            }
            if (notExistMissions.Count > 0)
                await userCtx.UserMissions.AddRangeAsync(notExistMissions);

        }

        private async Task UpdateAchievMissionDbCountUpAsync(UserCtx userCtx, CsvStoreContextData csvData, long userSeq, List<CountUpMission> addMissionList)
        {
            var indexList = addMissionList.Select(p => p.MissionIndex).ToList();
            var userMissions = await userCtx.UserAchievements.Where(p => p.UserSeq == userSeq && indexList.Contains(p.MissionIndex)).ToListAsync();

            var notExistMissions = new List<UserAchievementModel>();

            foreach (var mission in addMissionList)
            {
                var missionDb = userMissions.Where(p => p.MissionIndex == mission.MissionIndex).FirstOrDefault();
                if (missionDb == null)
                {
                    missionDb = new UserAchievementModel
                    {
                        UserSeq = userSeq,
                        MissionIndex = mission.MissionIndex,
                        MissionCount = mission.Count,
                        LastRewardOrderNum = 0,
                    };
                    notExistMissions.Add(missionDb);
                }
                else
                {
                    missionDb.MissionCount += mission.Count;
                    userCtx.UserAchievements.Update(missionDb);
                }
                //var missionData = csvData.MissionAchievementListData.Where(p => p.MissionGroupIndex == mission.MissionIndex && p.MissionOrder == missionDb.LastRewardOrderNum + 1).FirstOrDefault();
                //if( missionData.Count <= missionDb.MissionCount)
                //{
                //    // 
                //}
            }
            if (notExistMissions.Count > 0)
                await userCtx.UserAchievements.AddRangeAsync(notExistMissions);
        }

        private async Task UpdateDailyMissionDbOverwriteAsync(UserCtx userCtx, long userSeq, List<OverwriteMission> addMissionList)
        {
            if (addMissionList.Count <= 0)
                return;
            var missionIndexList = addMissionList.Select(p => p.MissionIndex).ToList();

            var userMissions = await userCtx.UserMissions.Where(p => p.UserSeq == userSeq && missionIndexList.Contains(p.MissionIndex)).ToListAsync();
            var notExistMissions = new List<UserMissionModel>();

            foreach (var addMission in addMissionList)
            {
                if (addMission.Count <= 0)
                    continue;

                var missionDb = userMissions.Where(p => p.MissionIndex == addMission.MissionIndex).FirstOrDefault();
                if (missionDb == null)
                {
                    notExistMissions.Add(new UserMissionModel
                    {
                        UserSeq = userSeq,
                        MissionIndex = addMission.MissionIndex,
                        MissionCount = addMission.Count,
                        IsReward = false,
                    });
                    
                }
                else
                {
                    if( missionDb.IsReward == false )
                    {
                        missionDb.MissionCount = addMission.Count;
                        userCtx.UserMissions.Update(missionDb);
                    }
                }
            }
            if (notExistMissions.Count > 0)
                await userCtx.UserMissions.AddRangeAsync(notExistMissions);
        }
        private async Task UpdateAchievMissionDbOverwriteAsync(UserCtx userCtx, long userSeq, List<OverwriteMission> addMissionList)
        {
            if (addMissionList.Count <= 0)
                return;
            var missionIndexList = addMissionList.Select(p => p.MissionIndex).ToList();
            var userMissions = await userCtx.UserAchievements.Where(p => p.UserSeq == userSeq && missionIndexList.Contains(p.MissionIndex)).ToListAsync();
            var notExistMissions = new List<UserAchievementModel>();

            foreach (var addMission in addMissionList)
            {
                if (addMission.Count <= 0)
                    continue;

                var missionDb = userMissions.Where(p => p.MissionIndex == addMission.MissionIndex).FirstOrDefault();
                if (missionDb == null)
                {
                    notExistMissions.Add(new UserAchievementModel
                    {
                        UserSeq = userSeq,
                        MissionIndex = addMission.MissionIndex,
                        MissionCount = addMission.Count,
                        LastRewardOrderNum = 0,
                    });
                }
                else
                {
                    missionDb.MissionCount = addMission.Count;
                    userCtx.UserAchievements.Update(missionDb);
                }
            }
            if (notExistMissions.Count > 0)
                await userCtx.UserAchievements.AddRangeAsync(notExistMissions);
        }

       
        public async Task AddMissionCountUpAsync(UserCtx userCtx, StoreEventCtx storeEventCtx, long userSeq, Cond1Type cond1, Cond2Type cond2, long index, BigInteger count)
        {
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            // 일일 미션
            var dailyMissionDatas = csvData.MissionDailyDicData.Values.Where(x => x.Cond1 == cond1 && x.Cond2 == cond2 && x.MissionDt.Date == AppClock.UtcNow.Date).ToList();
            if( dailyMissionDatas.Any() )
            {
                var addCountUpDailyMissionList = new List<CountUpMission>();
                foreach (var dailyMission in dailyMissionDatas)
                {
                    var cond3 = long.Parse(dailyMission.Cond3);
                    if( cond3 == 0 || cond3 == index)
                        addCountUpDailyMissionList.Add(new CountUpMission(dailyMission.Index, count));
                }
                if (addCountUpDailyMissionList.Count > 0)
                {
                    addCountUpDailyMissionList = addCountUpDailyMissionList.GroupBy(x => x.MissionIndex)
                        .Select(t => new CountUpMission(t.Key, t.Aggregate<CountUpMission, BigInteger>(0, (value1, value2) => value1 + value2.Count))).ToList();
                    //addCountUpDailyMissionList = addCountUpDailyMissionList.GroupBy(x => x.MissionIndex).Select(t => new CountUpMission(t.Key, t.Sum(e => e.Count))).ToList();
                    await UpdateDailyMissionDbCountUpAsync(userCtx, csvData, userSeq, addCountUpDailyMissionList);
                }
                    
            }
            // 업적 미션
            var achievGroupIndexList = csvData.MissionAchievementListData.Where(x => x.Cond1 == cond1 && x.Cond2 == cond2).Select(x=>x.MissionGroupIndex).Distinct().ToList();
            if(achievGroupIndexList.Any())
            {
                var addCountUpAchievMissionList = new List<CountUpMission>();
                foreach (var achievGroupIndex in achievGroupIndexList)
                {
                    var achievData = csvData.MissionAchievementListData.Where(p => p.MissionGroupIndex == achievGroupIndex).First();
                    var cond3 = long.Parse(achievData.Cond3);
                    if( cond3 == 0 || cond3 == index)
                        addCountUpAchievMissionList.Add(new CountUpMission(achievGroupIndex, count));
                }
                if (addCountUpAchievMissionList.Count > 0)
                {
                    addCountUpAchievMissionList = addCountUpAchievMissionList.GroupBy(x => x.MissionIndex)
                        .Select(t => new CountUpMission(t.Key, t.Aggregate<CountUpMission, BigInteger>(0, (value1, value2) => value1 + value2.Count))).ToList();
                    //addCountUpAchievMissionList = addCountUpAchievMissionList.GroupBy(x => x.MissionIndex).Select(t => new CountUpMission(t.Key, t.Sum(e => e.Count))).ToList();
                    await UpdateAchievMissionDbCountUpAsync(userCtx, csvData, userSeq, addCountUpAchievMissionList);
                }
                    
            }
            
            

            // 네비게이션 미션

            var naviMission = await GetNavigationMissionAsync(redisUser, userSeq);

            if( naviMission.MissionIndex > 0 )
            {
                var naviData = csvData.MissionNavigationListData.Where(p => p.Index == naviMission.MissionIndex).FirstOrDefault();

                if (naviData != null && naviData.Cond1 == cond1 && naviData.Cond2 == cond2)
                {
                    var cond3 = long.Parse(naviData.Cond3);
                    if (cond3 == 0 || cond3 == index)
                    {
                        var naviDb = await userCtx.UserNavigationMissions.Where(p => p.UserSeq == userSeq && p.MissionIndex == naviMission.MissionIndex).FirstOrDefaultAsync();
                        if (naviDb != null)
                        {
                            if (naviData.Count > naviDb.MissionCount)
                            {
                                naviDb.MissionCount += count;
                                userCtx.UserNavigationMissions.Update(naviDb);

                                var naviRedisKey = string.Format(RedisKeys.hs_NavigationMission, userSeq);
                                var naviCountRedis = await redisUser.HashGetAsync(naviRedisKey, "count");
                                var naviCountString = (string)naviCountRedis;
                                var updateNaviCount = BigInteger.Parse(naviCountString, System.Globalization.NumberStyles.Number) + count;
                                await redisUser.HashSetAsync(naviRedisKey, "count", updateNaviCount.ToString());
//                                await redisUser.HashIncrementAsync(naviRedisKey, "count", count);
                            }
                        }
                    }
                }
            }
        }

        public async Task AddMissionOverwirteAsync(UserCtx userCtx, StoreEventCtx storeEventCtx, long userSeq, Cond1Type cond1, Cond2Type cond2, long index, BigInteger num)
        {
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            // 일일 미션
            var dailyMissionDatas = csvData.MissionDailyDicData.Values.Where(x => x.Cond1 == cond1 && x.Cond2 == cond2 && x.MissionDt.Date == AppClock.UtcNow.Date).ToList();
            if (dailyMissionDatas.Any())
            {
                var addOverwriteDailyMissionList = new List<OverwriteMission>();                
                foreach (var dailyMission in dailyMissionDatas)
                {
                    var cond3 = long.Parse(dailyMission.Cond3);
                    if (cond3 == 0 || cond3 == index)
                        addOverwriteDailyMissionList.Add(new OverwriteMission(dailyMission.Index, num));
                }
                if (addOverwriteDailyMissionList.Count > 0)
                    await UpdateDailyMissionDbOverwriteAsync(userCtx, userSeq, addOverwriteDailyMissionList);
            }
            // 업적 미션
            var achievGroupIndexList = csvData.MissionAchievementListData.Where(x => x.Cond1 == cond1 && x.Cond2 == cond2).Select(x => x.MissionGroupIndex).Distinct().ToList();
            if (achievGroupIndexList.Any())
            {   
                var addOverwriteAchievMissionList = new List<OverwriteMission>();
                foreach (var achievGroupIndex in achievGroupIndexList)
                {
                    var achievData = csvData.MissionAchievementListData.Where(p => p.MissionGroupIndex == achievGroupIndex).First();
                    var cond3 = long.Parse(achievData.Cond3);
                    if (cond3 == 0 || cond3 == index)
                        addOverwriteAchievMissionList.Add(new OverwriteMission(achievGroupIndex, num));
                }
                if (addOverwriteAchievMissionList.Count > 0)
                    await UpdateAchievMissionDbOverwriteAsync(userCtx, userSeq, addOverwriteAchievMissionList);
            }

            
            // 네비게이션 미션
            var naviMission = await GetNavigationMissionAsync(redisUser, userSeq);
            if( naviMission.MissionIndex > 0 )
            {
                var naviData = csvData.MissionNavigationListData.Where(p => p.Index == naviMission.MissionIndex).FirstOrDefault();

                if (naviData != null && naviData.Cond1 == cond1 && naviData.Cond2 == cond2)
                {
                    var cond3 = long.Parse(naviData.Cond3);
                    if (cond3 == 0 || cond3 == index)
                    {
                        var naviDb = await userCtx.UserNavigationMissions.Where(p => p.UserSeq == userSeq && p.MissionIndex == naviMission.MissionIndex).FirstOrDefaultAsync();
                        if (naviDb != null)
                        {
                            naviDb.MissionCount = num;
                            userCtx.UserNavigationMissions.Update(naviDb);

                            await redisUser.HashSetAsync(string.Format(RedisKeys.hs_NavigationMission, userSeq), "count", num.ToString());

                        }
                    }
                }
            }
        }

       

       
        public async Task<BigInteger> GetNaviMissionCountAsync(UserCtx userCtx, long userSeq, MissionNavigationCSVData naviData)
        {
            BigInteger missionCount = 0;

            if (naviData.Cond1 == Cond1Type.Level)
            {
                switch (naviData.Cond2)
                {
                    case Cond2Type.ShoppingMall:
                        var userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);
                        missionCount = userInfoDb.Level;
                        break;

                   
                }
            }
            else if (naviData.Cond1 == Cond1Type.Grade)
            {
                if (naviData.Cond2 == Cond2Type.ShoppingmallGrade)
                {
                    var userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);
                    missionCount = userInfoDb.Grade;
                }
            }

            return missionCount;
        }
        public async Task<bool> DailyMissionRedDotCheckAsync(UserCtx userCtx, long userSeq)
        {
            var csvData = _csvContext.GetData();
            var dailyMissionDatas = csvData.MissionDailyDicData.Values.Where(p => p.MissionDt.Date == AppClock.UtcNow.Date).ToList();
            var dailyIndexList = dailyMissionDatas.Select(p => p.Index).ToList();

            var missionDbs = await userCtx.UserMissions.Where(p => p.UserSeq == userSeq && dailyIndexList.Contains(p.MissionIndex)).ToListAsync();

            foreach (var mission in dailyMissionDatas)
            {
                var db = missionDbs.Where(p => p.MissionIndex == mission.Index).FirstOrDefault();
                var count = db == null ? 0 : db.MissionCount;
                var isReward = db == null ? false : db.IsReward;

                if (isReward == false && count >= mission.Count)
                    return true;
            }
            return false;

            //var dailyMisssions = await GetDailyMissionInfosAsync(userCtx, userSeq);
            //return dailyMisssions.Any(p => p.IsReward == false && p.CurrentCount >= p.MaxCount);
        }

        public async Task<bool> AchiveMissionRedDotCheckAsync(UserCtx userCtx, long userSeq)
        {  
            var csvData = _csvContext.GetData();            
            var achievMisssions = await GetAchievMissionInfosAsync(userCtx, userSeq);

            foreach(var mission in achievMisssions)
            {   
                var data = csvData.MissionAchievementListData.Where(p => p.MissionGroupIndex == mission.MissionIndex && p.MissionOrder == mission.LastRewardOrderNum + 1).FirstOrDefault();
                if (data == null)
                    continue;
                if (data.Count <= mission.CurrentCount)
                    return true;
            }
            return false;
        }

        public async Task<MissionBase> GetNavigationMissionAsync(IDatabase redisUser, long userSeq)
        {
            var naviRedis = await redisUser.HashGetAllAsync(string.Format(RedisKeys.hs_NavigationMission, userSeq));
            if (naviRedis.Any())
            {
                var dataNaviMissionDic = naviRedis
               .Select(e => new { key = (string)e.Name, Value = e.Value })
               .ToDictionary(e => e.key, e => e.Value);

                var missionString = (string)dataNaviMissionDic["count"];
                var missionCount = BigInteger.Parse(missionString, System.Globalization.NumberStyles.Number);
                var naviMission = new MissionBase
                {
                    MissionIndex = (long)dataNaviMissionDic["missionIndex"],
                    CurrentCount = missionCount
                };
                return naviMission;
            }
            else
                return new MissionBase();
            
        }

    }
}
