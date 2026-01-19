using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Nest;
using Newtonsoft.Json.Linq;
using RedLockNet.SERedis;
using Shared;
using Shared.CdnStore;
using Shared.Clock;
using Shared.CsvData;
using Shared.CsvParser.Extensions;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.Packet.Server.Extensions;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerModel;
using Shared.Services.Redis;
using StackExchange.Redis;
using SampleGame.Shared.Common;


namespace Shared.ServerApp.Services
{
    public class PlayerService
    {
        private readonly ILogger<PlayerService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedLockFactory _redLockFactory;        
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;        
        private readonly InventoryService _inventoryService;  
        private readonly MissionService _missionService;
        


        private List<RankInfo> _shoppingmallRankInfos = new List<RankInfo>();

        public PlayerService(
            ILogger<PlayerService> logger,
            DatabaseRepositoryService dbRepo,
            RedLockFactory redLockFactory,            
            CsvStoreContext csvStoreContext,             
            InventoryService inventoryService,
            MissionService missionService,
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _redLockFactory = redLockFactory;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;
            _inventoryService = inventoryService;
            _missionService = missionService;
        }

        public void SetShoppingmallRankInfo(List<RankInfo> rankInfos) => _shoppingmallRankInfos = rankInfos;
        
        public RankInfo GetShoppingmallUserRankInfo(long userSeq)
        {
            return _shoppingmallRankInfos.Where(p=>p.UserInfo.UserSeq == userSeq).FirstOrDefault();            
        }

        public async Task<UserInfoDetail> GetUserInfoDetailAsync(UserCtx userCtx, long userSeq, long targetUserSeq)
        {
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            var csvData = _csvContext.GetData();

            var userInfoRedis = await redisUser.HashGetAllAsync(string.Format(RedisKeys.hs_UserInfo, targetUserSeq));
            if (userInfoRedis.Length <= 0)
                userInfoRedis = await SetUserInfoRedisAsync(userCtx, redisUser, targetUserSeq);

            var userInfoDic = userInfoRedis
           .Select(e => new { key = (string)e.Name, Value = e.Value })
           .ToDictionary(e => e.key, e => e.Value);

            var shoppingmallRank = 0;
            var shoppingmallRankScore = 0L;

            var shoppingmallRankInfo = GetShoppingmallUserRankInfo(targetUserSeq);
            if( shoppingmallRankInfo == null )
            {
                var shoppingmallRankRedis = await redisRank.SortedSetRankAsync(RedisKeys.ss_ShoppingmallRank, targetUserSeq, Order.Descending);
                var shoppingmallScoreRedis = await redisRank.SortedSetScoreAsync(RedisKeys.ss_ShoppingmallRank, targetUserSeq);

                shoppingmallRank = shoppingmallRankRedis.HasValue ? (int)shoppingmallRankRedis + 1 : 0;
                shoppingmallRankScore = shoppingmallScoreRedis.HasValue ? (long)shoppingmallScoreRedis : 0;
            }
            else
            {
                shoppingmallRank = shoppingmallRankInfo.Rank;
                shoppingmallRankScore = (long)shoppingmallRankInfo.Score;
            }


            var redisTick = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, targetUserSeq);
            var lastConnectTick = redisTick.HasValue ? (long)redisTick : 0;

            return new UserInfoDetail
            {
                UserSeq = (long)userInfoDic["userSeq"],
                Level = (int)userInfoDic["level"],
                Nick = userInfoDic["nick"],
                ShoppingmallGrade = (int)userInfoDic["grade"],
                RepresentPostingSeq = (long)userInfoDic["representPostingSeq"],
                Comment = userInfoDic["comment"],
                LatestConnectDtTick = lastConnectTick,
                ShoppingmallRank = shoppingmallRank,
                ManagePoint = shoppingmallRankScore,
                
                ProfileInfo = ToProfileInfo((long)userInfoDic["userSeq"], userInfoDic["profileParts"]),
            };
        }

        public async Task <string> GetUserCodeAsync(UserCtx userCtx)
        {
            var userCode = string.Empty;
            var codeCreate = false;
            for (int i = 0; i < 100; i++)
            {
                var randGuid = Guid.NewGuid().ToString().ToUpper();
                var replaceResult = randGuid.Replace("-", "");
                var charList = new List<char>(replaceResult);
                var shuffleCharList = charList.OrderBy(p => Guid.NewGuid()).ToList();
                var stringBuilder = new StringBuilder();
                for (int j = 0; j < 8; j++)
                    stringBuilder.Append(shuffleCharList[j]);
                userCode = stringBuilder.ToString();
                var duplicate = await userCtx.UserFriendInfos.AnyAsync(p => p.UserCode == userCode);
                if (duplicate == false)
                {
                    codeCreate = true;
                    break;
                }
            }

            if (codeCreate == false)
                return null;

            return userCode;

        }

        public async Task<HashEntry[]> SetUserInfoRedisAsync(UserCtx userCtx, IDatabase redisUser, long userSeq, UserInfoModel userInfoDb = null)
        {
            if (userInfoDb == null)
                userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);

            var comment = userInfoDb.Comment == null ? "" : userInfoDb.Comment;
            var hashArray = new[]
            {
                new HashEntry("userSeq", userInfoDb.UserSeq),
                new HashEntry("profileParts", userInfoDb.ProfileParts),
                new HashEntry("nick", userInfoDb.Nick),
                new HashEntry("level",userInfoDb.Level),
                new HashEntry("grade",userInfoDb.Grade),
                new HashEntry("updateCurrencyChargeDtTick", userInfoDb.UpdateCurrencyChargeQtyDt.Ticks),
                new HashEntry("loginCount", userInfoDb.LoginCount),
//                new HashEntry("lastConnectDtTick", AppClock.UtcNow.Ticks),

            };
            await redisUser.HashSetAsync(string.Format(RedisKeys.hs_UserInfo, userSeq), hashArray);

            return hashArray;
        }

        public async Task<UserInfo> GetUserInfoModelAsync(UserCtx userCtx, IDatabase redisUser, long userSeq)
        {   
            var redisValueArray = new[]
            {   
                new RedisValue("nick"),
                new RedisValue("level"),
                new RedisValue("grade"),
                new RedisValue("influencer"),
                new RedisValue("profileParts"),
            };

            var redisUserInfoValues = await redisUser.HashGetAsync(string.Format(RedisKeys.hs_UserInfo, userSeq), redisValueArray);

            if( redisUserInfoValues[0].HasValue == true)
            {
                
                return new UserInfo
                {
                    UserSeq = userSeq,
                    Nick = redisUserInfoValues[0].HasValue ? (string)redisUserInfoValues[0] : "",
                    Level = redisUserInfoValues[1].HasValue ? (int)redisUserInfoValues[1] : 1,
                    ShoppingmallGrade = redisUserInfoValues[2].HasValue ? (int)redisUserInfoValues[2] : 1,
                    ProfileInfo = ToProfileInfo(userSeq, redisUserInfoValues[4].HasValue ? (string)redisUserInfoValues[4] : ""),
                };
            }
            else
            {
                var userInfoRedis = await SetUserInfoRedisAsync(userCtx, redisUser, userSeq);
                var userInfoDic = userInfoRedis
               .Select(e => new { key = (string)e.Name, Value = e.Value })
               .ToDictionary(e => e.key, e => e.Value);

                return new UserInfo
                {
                    UserSeq = (long)userInfoDic["userSeq"],
                    Nick = userInfoDic["nick"],
                    Level = (int)userInfoDic["level"],
                    ShoppingmallGrade = (int)userInfoDic["grade"],
                    ProfileInfo = ToProfileInfo((long)userInfoDic["userSeq"], userInfoDic["profileParts"]),
                };
            }
        }

       

        public async Task<List<UserInfo>> GetUserInfosAsync(UserCtx userCtx, List<long> userSeqs)
        {
            var userInfoDbs = await userCtx.UserInfos.Where(p => userSeqs.Contains(p.UserSeq)).ToListAsync();

            var userInfos = new List<UserInfo>();   
            foreach(var userInfoDb in userInfoDbs)
            {                
                var userInfo = await GetUserInfoAsync(userCtx, userInfoDb, isSelectCrew: false);
                userInfos.Add(userInfo);
            }
            return userInfos;
        }

        public async Task<UserInfo> GetUserInfoAsync(UserCtx userCtx, UserInfoModel userInfoDb, bool isSelectCrew = true)
        {
            var userInfo = ToUserInfo(userInfoDb);

            return userInfo;
        }

       

        public async Task SetShoppingmallRankAsync()
        {
            using var userCtx = _dbRepo.GetUserDb();
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            var rankInfos = new List<RankInfo>();

            var redisRankData = await redisRank.SortedSetRangeByRankWithScoresAsync(RedisKeys.ss_ShoppingmallRank, 0, 99, Order.Descending);
            
            // 랭킹 미존재 일 경우 
            if (redisRankData == null || redisRankData.Length <= 0)
            {
                SetShoppingmallRankInfo(rankInfos);
                return;
            }   

            // 랭킹 정보 있을 시 UserSimple 데이터와 랭킹 정보 전달

            var userSeqs = redisRankData.Select(x => (long)x.Element).ToList();
            var userInfoList = await GetUserInfosAsync(userCtx, userSeqs);

            int rank = 1;
            foreach (var rankUser in redisRankData)
            {
                var score = (long)rankUser.Score;
                rankInfos.Add(new RankInfo
                {
                    Rank = rank,
                    Score = score,
                    UserInfo = userInfoList.Where(x => x.UserSeq == (long)rankUser.Element).FirstOrDefault(),
                });
                rank++;
            }
            SetShoppingmallRankInfo(rankInfos);
        }

        public ProfileInfo ToProfileInfo(long userSeq, string parts)
        {
            return new ProfileInfo
            {
                Parts = string.IsNullOrEmpty(parts) ? new List<PartsBase>() : JsonTextSerializer.Deserialize<List<PartsBase>>(parts),
                
            };
        }

        
        public UserInfo ToUserInfo(UserInfoModel info)
        {
            return new UserInfo
            {
                UserSeq = info.UserSeq,
                Nick = info.Nick,
                Level = info.Level,
                ShoppingmallGrade = info.Grade,
                ProfileInfo = ToProfileInfo(info.UserSeq, info.ProfileParts),
            };
        }

        public async Task<List<CurrencyInfo>> ChargeCurrencyAsync(UserCtx userCtx, IDatabase redisUser ,long userSeq, long currencyChargeTick = 0, bool isSave = true)
        {
            var csvData = _csvContext.GetData();
            var chargeCurrecys = new List<CurrencyInfo>();

            var updateCurrencyChargeDt = AppClock.UtcNow;
            if ( currencyChargeTick > 0 )
            {
                updateCurrencyChargeDt = new DateTime(currencyChargeTick, DateTimeKind.Utc);
            }
            else
            {
                var updateCurrencyChargeDtTick = AppClock.UtcNow.Ticks;
                var updateCurrencyChargeDtTickRedis = await redisUser.HashGetAsync(string.Format(RedisKeys.hs_UserInfo, userSeq), "updateCurrencyChargeDtTick");
                if (updateCurrencyChargeDtTickRedis.HasValue == false)
                {
                    var userInfoRedis = await SetUserInfoRedisAsync(userCtx, redisUser, userSeq);

                    var userInfoDic = userInfoRedis
                       .Select(e => new { key = (string)e.Name, Value = e.Value })
                       .ToDictionary(e => e.key, e => e.Value);

                    updateCurrencyChargeDtTick = (long)userInfoDic["updateCurrencyChargeDtTick"];
                }
                else
                    updateCurrencyChargeDtTick = (long)updateCurrencyChargeDtTickRedis;

                updateCurrencyChargeDt = new DateTime(updateCurrencyChargeDtTick, DateTimeKind.Utc);
            }
            

            // 날짜 변동으로 재화 충전 
            if (updateCurrencyChargeDt.Day != AppClock.UtcNow.Day)
            {
                var chargeIndexList = csvData.CurrencyDicData.Values.Where(p => p.ChargeMax > 0).Select(p => p.Index).ToList();
                var currencyDbs = await userCtx.UserCurrency.Where(p => p.UserSeq == userSeq && chargeIndexList.Contains(p.ItemId)).ToListAsync();

                foreach(var db in currencyDbs)
                {
                    if (csvData.CurrencyDicData.TryGetValue(db.ItemId, out var data) == false)
                        continue;
                    if(db.ItemQty < data.ChargeMax)
                    {
                        db.ItemQty = data.ChargeMax;
                        chargeCurrecys.Add(new CurrencyInfo((CurrencyType)db.ItemId, db.ItemQty));
                    }
                }

                userCtx.UserCurrency.UpdateRange(currencyDbs);
                var userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);
                userInfoDb.UpdateCurrencyChargeQtyDt = AppClock.UtcNow;

                userCtx.UserInfos.Update(userInfoDb);
                if(isSave == true)
                    await userCtx.SaveChangesAsync();
                await redisUser.HashSetAsync(string.Format(RedisKeys.hs_UserInfo, userSeq), "updateCurrencyChargeDtTick", userInfoDb.UpdateCurrencyChargeQtyDt.Ticks);
            }

            return chargeCurrecys;
        }

        public async Task<int> GetUserGradeAsync(UserCtx userCtx, IDatabase redisUser, long userSeq)
        {
            var grade = 1;
            var gradeRedis = await redisUser.HashGetAsync(string.Format(RedisKeys.hs_UserInfo, userSeq), "grade");
            if (gradeRedis.HasValue == false)
            {
                var userInfoRedis = await SetUserInfoRedisAsync(userCtx, redisUser, userSeq);

                var userInfoDic = userInfoRedis
                   .Select(e => new { key = (string)e.Name, Value = e.Value })
                   .ToDictionary(e => e.key, e => e.Value);

                grade = (int)userInfoDic["grade"];
            }
            else
                grade = (int)gradeRedis;
            return grade;
        }

        public BigInteger GetCalcGoldRewardAmount(int grade, decimal rewardAmount)
        {
            var csvData = _csvContext.GetData();
            var rewardGoldVale = BigInteger.One;
            if (csvData.ShoppingmallGradeDicData.TryGetValue(grade, out var gradeData))
                rewardGoldVale = gradeData.RewardGoldValue;

            return ((BigInteger)(rewardAmount * 10000) * rewardGoldVale) / 10000;
        }

        public ItemInfo GetCalcGoldItemInfo(int grade, ItemInfo rewardInfo)
        {
            var csvData = _csvContext.GetData();
            var rewardGoldVale = BigInteger.One;
            if (csvData.ShoppingmallGradeDicData.TryGetValue(grade, out var gradeData))
                rewardGoldVale = gradeData.RewardGoldValue;

            if (rewardInfo.ItemType == RewardPaymentType.Currency && rewardInfo.Index == (long)CurrencyType.Gold)
                return new ItemInfo(rewardInfo.ItemType, rewardInfo.Index, (rewardInfo.ItemQty * rewardGoldVale));

            return rewardInfo;
        }

        public List<ItemInfo> GetCalcGoldItemInfos(int grade, List<ItemInfo> rewardInfos)
        {   
            var csvData = _csvContext.GetData();
            var rewardGoldVale = BigInteger.One;
            if (csvData.ShoppingmallGradeDicData.TryGetValue(grade, out var gradeData))
                rewardGoldVale = gradeData.RewardGoldValue;

            var result = new List<ItemInfo>();  
            foreach(var reward in rewardInfos)
            {
                if( reward.ItemType == RewardPaymentType.Currency && reward.Index == (long)CurrencyType.Gold)
                {
                    result.Add(new ItemInfo(reward.ItemType, reward.Index, reward.ItemQty * rewardGoldVale));
                }
                else
                {
                    result.Add(reward);
                }
            }
            return result;
        }

    }
}
