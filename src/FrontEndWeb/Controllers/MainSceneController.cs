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
using Shared.Entities.Models;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using StackExchange.Redis;
using Shared.PacketModel;
using Shared.Packet.Models;
using Shared.ServerApp.Utility;
using RedLockNet.SERedis;
using System;
using SampleGame.Shared.Common;
namespace FrontEndWeb.Controllers
{    
    [Route("api/mainscene")]
    public class MainSceneController : TokenBasedApiController
    {
        private readonly ILogger<MainSceneController> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;
        private readonly CsvStoreContext _csvContext;
        private readonly ServerInspectionService _serverInspectionService;
        private readonly MissionService _missionService;
        private readonly InventoryService _inventoryService;
        private readonly AttendanceService _attendanceService;
        private readonly PlayerService _playerService;
        private readonly RedLockFactory _redLockFactory;

        public MainSceneController(
            ILogger<MainSceneController> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvContext,
            RedisRepositoryService redisRepo,
            ServerInspectionService serverInspectionService,
            MissionService missionService,
            InventoryService inventoryService,
            AttendanceService attendanceService,    
            PlayerService playerService,
            RedLockFactory redLockFactory
        )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvContext;
            _redisRepo = redisRepo;
            _serverInspectionService = serverInspectionService;
            _missionService = missionService;
            _inventoryService = inventoryService;   
            _attendanceService = attendanceService; 
            _playerService = playerService; 
            _redLockFactory = redLockFactory;
        }

        [HttpPost("mainscene-enter")]
        public async Task<ActionResult<MainSceneEnterRes>> MainSceneEnterAsync([FromBody] MainSceneEnterReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<MainSceneEnterRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();

            using var userCtx = _dbRepo.GetUserDb();            

            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var csvData = _csvContext.GetData();

            var redDotTypes = new List<MainSceneRedDotType>();
            // 우편함 레드닷
            //if( await userCtx.UserMails.CountAsync(p => p.UserSeq == userSeq && p.IsObtain == false && p.LimitDt > AppClock.UtcNow) > 0)
            //    redDotTypes.Add(MainSceneRedDotType.Mail);

            
            var resp = new MainSceneEnterRes
            {
                RedDots = redDotTypes,
              
            };

            return Ok(ResultCode.Success, resp);
        }
      
        [HttpPost("use-coupon")]
        public async Task<ActionResult<UseCouponRes>> UseCouponAsync([FromBody] UseCouponReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<UseCouponRes>(ResultCode.ServerInspection);

            var csvData = _csvContext.GetData();
            var userSeq = GetUserSeq();
            var osType = GetUserOsType();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();

            if (csvData.CouponDicData.TryGetValue(req.CouponCode, out var data) == false)
                return Ok<UseCouponRes>(ResultCode.WrongCouponCode);
            if (data.IsValidTime(AppClock.UtcNow) == false)
                return Ok<UseCouponRes>(ResultCode.NotCouponTime);

            using var redLock = await _redLockFactory.CreateLockAsync(RedLockKeys.UseCouponLock(userSeq), TimeSpan.FromSeconds(5));
            if (redLock.IsAcquired == false)
                return Ok<UseCouponRes>(ResultCode.UsedCouponCode);

            if (data.CouponType == CouponType.All)
            {
                var userCouponDb = await userCtx.UserCoupons.Where(p => p.UserSeq == userSeq && p.CouponCode == req.CouponCode).FirstOrDefaultAsync();
                if (userCouponDb != null)
                    return Ok<UseCouponRes>(ResultCode.UsedCouponCode);

                var useCount = await userCtx.UserCoupons.CountAsync(p =>p.CouponCode == req.CouponCode);
                if( useCount >= data.RewardGiveNum)
                    return Ok<UseCouponRes>(ResultCode.LimitCouponCount);

                await userCtx.UserCoupons.AddAsync(new UserCouponModel
                {
                    UserSeq = userSeq,
                    CouponCode = req.CouponCode,
                });

            }
            else if (data.CouponType == CouponType.User)
            {
                var userCouponDb = await userCtx.UserCoupons.Where(p => p.UserSeq == userSeq && p.CouponCode == req.CouponCode).FirstOrDefaultAsync();
                if( userCouponDb != null )
                {
                    if( userCouponDb.UseCount >= data.RewardGiveNum )
                    {
                        if( data.RewardGiveNum <= 1 )
                            return Ok<UseCouponRes>(ResultCode.UsedCouponCode);
                        else
                            return Ok<UseCouponRes>(ResultCode.LimitCouponCount);
                    }
                        

                    userCouponDb.UseCount++;
                    userCtx.UserCoupons.Update(userCouponDb);
                }
                else
                {
                    await userCtx.UserCoupons.AddAsync(new UserCouponModel
                    {
                        UserSeq = userSeq,
                        CouponCode = req.CouponCode,
                    });
                }
            }
            var grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);            
            var rewards = _playerService.GetCalcGoldItemInfos(grade, data.rewardInfoList);

            var rewardUseCouponResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"useCoupon:{req.CouponCode}", rewards);
            if (rewardUseCouponResult.resultCode != ResultCode.Success)
                return Ok<UseCouponRes>(rewardUseCouponResult.resultCode);


            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new UseCouponRes
            {
                RefreshInfo = rewardUseCouponResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo(),
                RewardsInfo = rewardUseCouponResult.obtainResult.RewardsInfo,
            });
        }

        [HttpPost("attendance-enter")]
        public async Task<ActionResult<AttendanceEnterRes>> AttendanceEnterAsync([FromBody] AttendanceEnterReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<AttendanceEnterRes>(ResultCode.ServerInspection);

            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();
            var userSeq = GetUserSeq();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            // 하루에 한번씩 충전되는 재화들 충전 
            var changeCurrencys = await _playerService.ChargeCurrencyAsync(userCtx, redisUser, userSeq);

            var attendInfo = await _attendanceService.AttendanceEnterAsync(userCtx, userSeq);
            if (attendInfo.IsReward(AppClock.UtcNow))
            {
                var userInfoDb = await userCtx.UserInfos.Where(p => p.UserSeq == userSeq).FirstOrDefaultAsync();
                if( userInfoDb != null)
                {
                    userInfoDb.LoginCount += 1;
                    userCtx.UserInfos.Update(userInfoDb);

                    await redisUser.HashSetAsync(string.Format(RedisKeys.hs_UserInfo, userSeq), "loginCount", userInfoDb.LoginCount);
                }
                await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Login, Cond2Type.None, 0, 1);
            }
//            await _storeService.GetCashbackProductSendMailAsync(userCtx, redisUser, userSeq);

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            var refreshInfo = new RefreshInventoryInfo();
            refreshInfo.CurrencyList.AddRange(changeCurrencys);

            return Ok(ResultCode.Success, new AttendanceEnterRes
            {
                AttendanceInfo = attendInfo,
                RefreshInfo = refreshInfo,
            });
        }

        [HttpPost("open-randombox")]
        public async Task<ActionResult<OpenRandomBoxRes>> OpenRandomBoxAsync([FromBody] OpenRandomBoxReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<OpenRandomBoxRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            if( req.BoxIndex <= 0 || req.OpenBoxCount <= 0 )
                return Ok<OpenRandomBoxRes>(ResultCode.InvalidParameter);

            if (csvData.BoxDicData.TryGetValue(req.BoxIndex, out var boxData) == false)
                return Ok<OpenRandomBoxRes>(ResultCode.NotFound);

            if( boxData.BoxType != BoxType.Random)
                return Ok<OpenRandomBoxRes>(ResultCode.InvalidParameter);

            //var isAvailable = await _inventoryService.AvailableOpenBoxAsync(userCtx, redisUser, userSeq, boxData.OpenQualificationType, boxData.OpenQualificationValue);
            //if (isAvailable == false)
            //    return Ok<OpenRandomBoxRes>(ResultCode.InvalidParameter);

            var useItems = new List<ItemInfo> { new ItemInfo(RewardPaymentType.Box, req.BoxIndex, req.OpenBoxCount) };

            var useResult = await _inventoryService.UseItemCurrencyAsync(userCtx, storeEventCtx, userSeq, "open-randombox", useItems);
            if (useResult.resultCode != ResultCode.Success)
                return Ok<OpenRandomBoxRes>(useResult.resultCode);

            var grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);

            var rewardInfos = WeightedRandomizer.From(boxData.RewardData).TakeMulti(req.OpenBoxCount);

            rewardInfos = _playerService.GetCalcGoldItemInfos(grade, rewardInfos);

            var obtainResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, "open-randombox", rewardInfos);
            if (obtainResult.resultCode != ResultCode.Success)
                return Ok<OpenRandomBoxRes>(obtainResult.resultCode);

            
            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new OpenRandomBoxRes
            {
                RewardsInfo = obtainResult.obtainResult.RewardsInfo,
                RefreshInfo = useResult.refreshItem.ToRefreshInventoryInfo().AddRefreshInfo(obtainResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo()),
            });
        }

        [HttpPost("open-fixbox")]
        public async Task<ActionResult<OpenFixBoxRes>> OpenFixBoxAsync([FromBody] OpenFixBoxReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<OpenFixBoxRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            if (req.BoxIndex <= 0 || req.OpenBoxCount <= 0)
                return Ok<OpenFixBoxRes>(ResultCode.InvalidParameter);

            if (csvData.BoxDicData.TryGetValue(req.BoxIndex, out var boxData) == false)
                return Ok<OpenFixBoxRes>(ResultCode.NotFound);

            if (boxData.BoxType != BoxType.Fix)
                return Ok<OpenRandomBoxRes>(ResultCode.InvalidParameter);


            //var isAvailable = await _inventoryService.AvailableOpenBoxAsync(userCtx, redisUser, userSeq, boxData.OpenQualificationType, boxData.OpenQualificationValue);
            //if (isAvailable == false)
            //    return Ok<OpenFixBoxRes>(ResultCode.InvalidParameter);

            var useItems = new List<ItemInfo> { new ItemInfo(RewardPaymentType.Box, req.BoxIndex, req.OpenBoxCount) };

            var useResult = await _inventoryService.UseItemCurrencyAsync(userCtx, storeEventCtx, userSeq, "open-fixbox", useItems);
            if (useResult.resultCode != ResultCode.Success)
                return Ok<OpenFixBoxRes>(useResult.resultCode);

            var rewardDatas = boxData.RewardData.Keys.ToList();
            var rewardInfos = new List<ItemInfo>();
            for (int i = 0; i < req.OpenBoxCount; i++)
            {
                rewardInfos.AddRange(rewardDatas);
            }

            var grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);

            rewardInfos = _playerService.GetCalcGoldItemInfos(grade, rewardInfos);

            var obtainResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, "open-fixbox", rewardInfos);
            if (obtainResult.resultCode != ResultCode.Success)
                return Ok<OpenFixBoxRes>(obtainResult.resultCode);


            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new OpenFixBoxRes
            {
                RewardsInfo = obtainResult.obtainResult.RewardsInfo,
                RefreshInfo = useResult.refreshItem.ToRefreshInventoryInfo().AddRefreshInfo(obtainResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo()),
            });
        }

        
    }
}


