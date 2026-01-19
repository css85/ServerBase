using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using Shared.Services.Redis;
using FrontEndWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Model;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using StackExchange.Redis;
using Shared.PacketModel;
using Shared.Packet.Models;
using Shared.ServerApp.Utility;
using Shared.Packet.Server.Extensions;
using SampleGame.Shared.Common;
using System;
using Shared.ServerModel;
using FrontEndWeb.Config;

namespace FrontEndWeb.Controllers
{
    //    [ApiVersion("2.0")]
    [Route("api/player")]
    public class PlayerController : TokenBasedApiController
    {
        private readonly ILogger<PlayerController> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;
        private readonly CsvStoreContext _csvContext;      
        private readonly ServerInspectionService _serverInspectionService;        
        private readonly PlayerService _playerService;
        private readonly MissionService _missionService;
        private readonly AttendanceService _attendanceService;
        private readonly FrontCheckService _frontCheckService;
        private readonly ChangeableSettings<FrontEndAppSettings> _appSettings;

        public PlayerController(
            ILogger<PlayerController> logger,
            ChangeableSettings<FrontEndAppSettings> appSettings,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvContext,
            RedisRepositoryService redisRepo,         
            InventoryService inventoryService,
            ServerInspectionService serverInspectionService,            
            PlayerService playerService,
            MissionService missionService,
            AttendanceService attendanceService,
            FrontCheckService frontCheckService    
        )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvContext;
            _redisRepo = redisRepo;
            _serverInspectionService = serverInspectionService;
            _playerService = playerService;
            _missionService = missionService;   
            _attendanceService = attendanceService;
            _frontCheckService = frontCheckService;
            _appSettings = appSettings; 
        }

        [HttpPost("signin")]
        public async Task<ActionResult<SignInRes>> SignInAsync([FromBody] SignInReq req)
        {
            var auth = HttpContext.Request.Headers.Authorization.FirstOrDefault();            
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<SignInRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            var osType = GetUserOsType();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var csvData = _csvContext.GetData();

            var userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);
            if (userInfoDb == null)
                return Ok<SignInRes>(ResultCode.NotFound);

            await _playerService.ChargeCurrencyAsync(userCtx, redisUser, userSeq, userInfoDb.UpdateCurrencyChargeQtyDt.Ticks, isSave : false);


            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            var currencyList = await userCtx.UserCurrency.Where(p => p.UserSeq == userSeq).Select(p => new CurrencyInfo((CurrencyType)p.ItemId, p.ItemQty)).ToListAsync();                
            var userItems = await userCtx.UserItems.Where(p => p.UserSeq == userSeq).ToListAsync();

            userItems = userItems.Where(p => p.ItemQty > 0).ToList();
            var attendanceInfo = await _attendanceService.AttendanceEnterAsync(userCtx, userSeq, userInfoDb.Grade);
            var accountLinks = await userCtx.UserAccountLinks.Where(p =>p.UserSeq == userSeq && p.AccountType > AccountType.Guest).Select(p=>p.AccountType).ToListAsync();
            var vipBase = await userCtx.UserVips.Where(p => p.UserSeq == userSeq).Select(p => p.ToVipBase()).FirstOrDefaultAsync();
            var userPoints = await userCtx.UserPoints.Where(p => p.UserSeq == userSeq).Select(p => p.ToItemInfo()).ToListAsync();
            var offlineRewardDb = await userCtx.UserOfflineRewards.FindAsync(userSeq);
            var offlineRewardInfo = offlineRewardDb != null ? new OfflineRewardInfo(offlineRewardDb.Rewarded, offlineRewardDb.OfflineTimeMin, JsonTextSerializer.Deserialize<List<ItemInfo>>(offlineRewardDb.RewardInfos)) : new OfflineRewardInfo();

            if( offlineRewardInfo.IsReward)
            {   
                var mailDbs = offlineRewardInfo.RewardInfos.Select(p => new UserMailModel
                {
                    UserSeq = userSeq,
                    ObtainType = p.ItemType,
                    ObtainId = p.Index,
                    ObtainQty = p.ItemQty,
                    TitleKey = "mailbox_offlinegift",
                    TitleKeyArg = "",
                    LimitDt = AppClock.UtcNow.AddDays(7),
                }).ToList();
                await userCtx.UserMails.AddRangeAsync(mailDbs);

                offlineRewardDb.Rewarded = true;
                userCtx.UserOfflineRewards.Update(offlineRewardDb);

            }

            if( attendanceInfo.IsReward(AppClock.UtcNow))
            {
                userInfoDb.LoginCount += 1;
                userCtx.UserInfos.Update(userInfoDb);
                await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Login, Cond2Type.None, 0, 1);
            }

            var userInfo = await _playerService.GetUserInfoAsync(userCtx, userInfoDb);
            //////////////////////////////////////////////////////////////////////////////////////////////////

            // 재화가 추가 됨에 따라 문제 발생 여부 체크
            // 없으면 추가 
            var userCurrencyModels = new List<UserCurrencyModel>();
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                if (type == CurrencyType.None || type == CurrencyType.Free)
                    continue;

                if( currencyList.Exists( p=>p.Currency == type) == false )
                {
                    var addCurrency = new UserCurrencyModel
                    {
                        UserSeq = userSeq,
                        ObtainType = RewardPaymentType.Currency,
                        ItemId = (long)type,
                        ItemQty = 0,
                    };
                    currencyList.Add(new CurrencyInfo(type, 0));
                }
            }
            await userCtx.UserCurrency.AddRangeAsync(userCurrencyModels);

            await redisUser.HashSetAsync(string.Format(RedisKeys.hs_UserVip, userSeq), new[]
            {
                new HashEntry("level", vipBase.Level),
                new HashEntry("endTick", vipBase.VipBenefitEndDtTick)
            });

            await redisUser.StringSetAsync(string.Format(RedisKeys.s_UserConnect, userSeq), userSeq, TimeSpan.FromSeconds(10));
            await _playerService.SetUserInfoRedisAsync(userCtx, redisUser, userSeq, userInfoDb);

            var loginCount = await redisUser.StringIncrementAsync(RedisKeys.GetTodayLoginCountKey(userSeq));
            if (loginCount == 1)
                await redisUser.KeyExpireAsync(RedisKeys.GetTodayLoginCountKey(userSeq), TimeSpan.FromDays(2));
            
            var resp = new SignInRes
            {
                UserInfo = userInfo,
                LoginCount = userInfoDb.LoginCount,
                CurrencyInfos = currencyList,
                ItemInfos = userItems.Select(x => x.ToItemInfo()).ToList(),
                ServerTimeTick = AppClock.UtcNow.Ticks,                
                
                AttendanceInfo = attendanceInfo,
                OfflineRewardInfo = offlineRewardInfo,
                PointInfos = userPoints,
                AccountLinks = accountLinks,
                VipBase = vipBase,
            };

            await userCtx.SignInHistory.AddAsync(new SignInHistoryModel
            {
                UserSeq = userSeq,
            });

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();
            return Ok(ResultCode.Success, resp);
        }

        [HttpPost("create-nick")]
        public async Task<ActionResult<CreateNickRes>> CreateNickAsync([FromBody] CreateNickReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<CreateNickRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();

            //if (string.IsNullOrEmpty(req.Nick))
            //    return Ok<CreateNickRes>(ResultCode.InvalidParameter);

            var userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);
            if( userInfoDb != null)
                return Ok<CreateNickRes>(ResultCode.ExistUserAccount);

            //if(await userCtx.UserInfos.AnyAsync(p=>p.Nick == req.Nick) == true)
            //    return Ok<CreateNickRes>(ResultCode.ExistNick);

            var accountDb = await userCtx.UserAccounts.FindAsync(userSeq);
            if( req.PushAgree == true )
            {
                accountDb.ServerPush = true;
                accountDb.NightPush = true;
            }
            else
            {
                accountDb.ServerPush = false;
                accountDb.NightPush = false;
            }
            userCtx.UserAccounts.Update(accountDb);

            var nick = string.Empty;
            var userSeqString = userSeq.ToString();
//            var nickRule = _gameRule.Value.NicknameString;
            var nickRule = req.Nick;

            int halfLength = userSeqString.Length / 2;

            if (halfLength <= 0)
                nick = $"{userSeqString}{nickRule}";
            else
            {
                nick = userSeqString.Insert(halfLength, "-");
                nick = $"{nick}{nickRule}";
            }

            // 회원가입 절차
            await userCtx.UserInfos.AddAsync(new UserInfoModel
            {
                UserSeq = userSeq,
                Nick = nick,
            });

            var userCurrencyModels = new List<UserCurrencyModel>();
            foreach(CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                if (type == CurrencyType.None || type == CurrencyType.Free)
                    continue;

                var qty = 0L;
                if (csvData.CurrencyDicData.TryGetValue((long)type, out var data))
                    qty = data.ChargeMax;

                if (type == CurrencyType.ADRemovalTicket)
                    qty = 10;
                
                    
                var currencyModel = new UserCurrencyModel
                {
                    UserSeq = userSeq,
                    ObtainType = RewardPaymentType.Currency,
                    ItemId = (long)type,
                    ItemQty = qty,
                };
                
                userCurrencyModels.Add(currencyModel);
            }
            await userCtx.UserCurrency.AddRangeAsync(userCurrencyModels);

            var userPointModels = new List<UserPointModel>();
            foreach (PointType type in  Enum.GetValues(typeof(PointType)))
            {
                if (type == PointType.None)
                    continue;
                userPointModels.Add(new UserPointModel
                {
                    UserSeq = userSeq, 
                    ObtainType = RewardPaymentType.Point,
                    ItemId = (long)type,
                    ItemQty = 0,
                });
            }
            await userCtx.UserPoints.AddRangeAsync(userPointModels);

            var achievGroupIndexList = csvData.MissionAchievementListData.Select(x => x.MissionGroupIndex).Distinct().ToList();

            var achievMissions = new List<UserAchievementModel>();

            foreach (var mission in achievGroupIndexList)
            {
                achievMissions.Add(new UserAchievementModel
                {
                    UserSeq = userSeq,
                    MissionIndex = mission,
                    MissionCount = 0,
                    LastRewardOrderNum = 0,
                });
            }
            await userCtx.UserAchievements.AddRangeAsync(achievMissions);


            await userCtx.UserVips.AddAsync(new UserVipModel
            {
                UserSeq = userSeq,
                LastRewardAttendanceDay = 0,
            });


            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();


            await _missionService.AddMissionOverwirteAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Level, Cond2Type.ShoppingMall, 0, 1);
            await _missionService.AddMissionOverwirteAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Grade, Cond2Type.ShoppingmallGrade, 0, 1);
            var userAttendDb = await _attendanceService.NewAttendanceAsync(userCtx, userSeq, AttendanceType.Newbie);
            
            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();
            // 회원 가입 절차 끝 

            return Ok(ResultCode.Success, new CreateNickRes
            {
                Nick = nick,
            });
        }

    
        [HttpPost("target-data")]
        public async Task<ActionResult<GetTargetDataRes>> TargetUserDataAsync([FromBody] GetTargetDataReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<GetTargetDataRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var userDetail = await _playerService.GetUserInfoDetailAsync(userCtx, userSeq, req.TargetUserSeq);
            if (userDetail == null)
                return Ok<GetTargetDataRes>(ResultCode.NotFound);

            var redisKey = RedisKeys.GetPostCommentCountKey(userSeq);
            var redisValue = await redisUser.StringGetAsync(redisKey);
            var commentCount = redisValue.HasValue ? (int)redisValue : 0;

            var friendState = FriendState.None;
            var isFriend = await userCtx.UserFriends.AnyAsync(p => p.UserSeq == userSeq && p.TargetSeq == req.TargetUserSeq);
            if( isFriend)
                friendState = FriendState.Friend;
            else
            {
                var requestFriend = await userCtx.UserReqestFriends.AnyAsync(p => p.UserSeq == userSeq && p.RequestUserSeq == req.TargetUserSeq);
                if(requestFriend)
                    friendState = FriendState.Request;
            }

            long representPostingCoolTimeDtTick = 0;
            if( userSeq == req.TargetUserSeq)
            {
                var coolTimeValue = await redisUser.StringGetAsync(string.Format(RedisKeys.s_RepresentPostingCoolTime, userSeq));
                if (coolTimeValue.HasValue)
                    representPostingCoolTimeDtTick = (long)coolTimeValue;
            }
           
            return Ok(ResultCode.Success, new GetTargetDataRes
            {
                UserInfoDetail = userDetail,            
                PostDailyCommentCount = commentCount,
                FriendState = friendState,
                RepresentPostingCoolTimeDtTick = representPostingCoolTimeDtTick,
            });

        }

    }
}


