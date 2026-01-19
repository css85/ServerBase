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
using System;
using Shared.ServerApp.Utility;
using Shared.Packet.Server.Extensions;
using SampleGame.Shared.Common;
using RedLockNet.SERedis;
using Shared.ServerModel;
using Shared.CsvParser.Extensions;
using Shared.CsvData;
using Shared.Repository.Extensions;
using System.Numerics;

namespace FrontEndWeb.Controllers
{
    [Route("api/store")]
    public class StoreController : TokenBasedApiController
    {
        private readonly ILogger<StoreController> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;
        private readonly CsvStoreContext _csvContext;
        private readonly InventoryService _inventoryService;
        private readonly ServerInspectionService _serverInspectionService;
        private readonly RedLockFactory _redLockFactory;
        private readonly IAPService _iapService;
        private readonly PlayerService _playerService;
        private readonly MissionService _missionService;

        public StoreController(
            ILogger<StoreController> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvContext,
            RedisRepositoryService redisRepo,
            InventoryService inventoryService,
            RedLockFactory redLockFactory,
            IAPService iapService,
            PlayerService playerService,    
            MissionService missionService,
            ServerInspectionService serverInspectionService
        )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvContext;
            _redisRepo = redisRepo;
            _inventoryService = inventoryService;
            _redLockFactory = redLockFactory;   
            _iapService = iapService;
            _playerService = playerService;
            _missionService = missionService;
            _serverInspectionService = serverInspectionService;
        }

        [HttpPost("enter")]
        public async Task<ActionResult<EnterStoreRes>> EnterStoreAsync([FromBody] EnterStoreReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<EnterStoreRes>(ResultCode.ServerInspection);
            
            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var gachaRareInfos =csvData.StoreGachaDicData.Values.Where(p=>p.IsValidTime(AppClock.UtcNow) && p.GachaType == GachaType.Rare).OrderBy(p=>p.Index).Select(p => p.ToGachaStoreInfo()).ToList();
            var gachaTimeLimitInfos = csvData.StoreGachaDicData.Values.Where(p => p.IsValidTime(AppClock.UtcNow) && p.GachaType == GachaType.TimeLimit).OrderByDescending(p => p.Index).Select(p => p.ToGachaStoreInfo()).ToList();
            var gachaSpecialInfos = csvData.StoreGachaDicData.Values.Where(p => p.IsValidTime(AppClock.UtcNow) && p.GachaType == GachaType.Special).OrderBy(p => p.Index).Select(p => p.ToGachaStoreInfo()).ToList();

            var gachaStoreInfos = new List<GachaStoreInfo>();
            gachaStoreInfos.AddRange(gachaRareInfos);
            gachaStoreInfos.AddRange(gachaTimeLimitInfos);
            gachaStoreInfos.AddRange(gachaSpecialInfos);

            var packageStoreInfos = csvData.StorePackageDicData.Values.Where(p => p.View && p.CycleType == CycleType.None).Select(p => p.ToPackageStoreInfo(0)).ToList();

            var buyPackDic = csvData.StorePackageDicData.Values
                .Where(p => p.View && p.CycleType != CycleType.None)
                .Select(p => KeyValuePair.Create(p, new RedisKey(RedisKeys.GetPackageKey(p.CycleType, p.Index, userSeq))))
                .ToDictionary(p => p.Key, p => p.Value);

            var keyArray = buyPackDic.Values.ToArray();
            var buyPackRedisDatas = await redisUser.StringGetAsync(keyArray);

            int idx = 0;
            foreach (var data in buyPackDic.Keys)
            {
                var buyPack = 0;
                if (buyPackRedisDatas[idx].HasValue)
                    buyPack = (int)buyPackRedisDatas[idx];
//                if (data.PurchaseLimitCount > buyPack)
                packageStoreInfos.Add(data.ToPackageStoreInfo(buyPack));
                idx++;
            }

            var resp = new EnterStoreRes 
            { 
               GachaStoreInfo = gachaStoreInfos,
               PackageInfo = packageStoreInfos,
            };
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
           
            return Ok(ResultCode.Success, resp);
        }

        [HttpPost("buygacha")]
        public async Task<ActionResult<BuyGachaRes>> BuyGachaAsync([FromBody] BuyGachaReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<BuyGachaRes>(ResultCode.ServerInspection);

            if (req.BuyCount <= 0)
                return Ok<BuyGachaRes>(ResultCode.InvalidParameter);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();

            var csvData = _csvContext.GetData();
            if (csvData.StoreGachaDicData.TryGetValue(req.StoreGachaIndex, out var data) == false)
                return Ok<BuyGachaRes>(ResultCode.NotSellProduct);
            if (data.IsValidTime(AppClock.UtcNow) == false)
                return Ok<BuyGachaRes>(ResultCode.NotSellProduct);

            if (data.ProbGroupData.Count <= 0)
                return Ok<BuyGachaRes>(ResultCode.NotSellProduct);

            var refreshGachaInfo = data.ToGachaStoreInfo();

            var rewardCount = req.BuyCount >= 10 ? req.BuyCount + 1 : req.BuyCount;

            var mileageRewards = new List<ItemInfo>();
            var rewardInfos = new List<ItemInfo>();

            //for (int i = 0; i < rewardCount; i++)
            //{
            //    var probGroupRandData = WeightedRandomizer.From(data.ProbGroupData).TakeOne();
            //    var rewardMileageRandData = WeightedRandomizer.From(probGroupRandData.RewardMileageData).TakeOne();

            //    var rewardData = probGroupRandData.RewardGroups.Where(p=>p.IsValidTime(AppClock.UtcNow)).Select(p => KeyValuePair.Create(p.RewardInfo, p.ProbWeight)).ToDictionary(p => p.Key, p => p.Value);

            //    rewardInfos.Add(WeightedRandomizer.From(rewardData).TakeOne());

            //    mileageRewards.Add(rewardMileageRandData);
            //}
            //mileageRewards = mileageRewards.GroupBy(x => new { type = x.ItemType, id = x.Index })
            //    .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Sum(e => e.ItemQty) })
            //    .ToList();
            mileageRewards = mileageRewards.GroupBy(x => new { type = x.ItemType, id = x.Index })
                .Select(t => new ItemInfo(t.Key.type, t.Key.id, t.Aggregate<ItemInfo, BigInteger>(0, (value1, value2) => value1 + value2.ItemQty)))
                .ToList();


            rewardInfos.AddRange(mileageRewards);

            var useGachaResult = await _inventoryService.UseItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"buygacha",
                new List<ItemInfo> { new ItemInfo(data.PaymentType_1, data.PaymentIndex_1, data.PaymentAmount_1 * req.BuyCount) });
            if (useGachaResult.resultCode != ResultCode.Success)
            {
                useGachaResult = await _inventoryService.UseItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"buygacha",
                new List<ItemInfo> { new ItemInfo(data.PaymentType_2, data.PaymentIndex_2, data.GetPaymentAmount2(req.BuyCount) * req.BuyCount) });

                if (useGachaResult.resultCode != ResultCode.Success)
                    return Ok<BuyGachaRes>(useGachaResult.resultCode);
            }


            var rewardGachaResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"buygacha", rewardInfos);
            if (rewardGachaResult.resultCode != ResultCode.Success)
            {
                return Ok<BuyGachaRes>(rewardGachaResult.resultCode);
            }

            var resp = new BuyGachaRes
            {
                RefreshInfo = useGachaResult.refreshItem.ToRefreshInventoryInfo().AddRefreshInfo(rewardGachaResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo()),
                RewardsInfo = rewardGachaResult.obtainResult.RewardsInfo,                
            };

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, resp);
        }

        [HttpPost("buypackage")]
        public async Task<ActionResult<BuyPackageRes>> BuyPackageAsync([FromBody] BuyPackageReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<BuyPackageRes>(ResultCode.ServerInspection);

            if (req.BuyCount <= 0)
                return Ok<BuyPackageRes>(ResultCode.InvalidParameter);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();

            if (csvData.StorePackageDicData.TryGetValue(req.StorePackageIndex, out var data) == false)
                return Ok<BuyPackageRes>(ResultCode.NotSellProduct);


            if (data.View == false)
                return Ok<BuyPackageRes>(ResultCode.NotSellProduct);
            if (data.RewardInfos.Count <= 0)
                return Ok<BuyPackageRes>(ResultCode.NotSellProduct);
            if( data.PaymentType == RewardPaymentType.Iap)
                return Ok<BuyPackageRes>(ResultCode.InvalidParameter);

            var refreshPackageInfo = new PackageStoreInfo
            {
                Index = data.Index,
                BuyCount = 0,
                Limit = data.PurchaseLimitCount,
                LimitCountCycle = data.CycleType,                
            };

            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var redisKeyExpire = false;
            // packageKey 가 null 일 경우는 제한이 없는 경우 
            string packageKey = RedisKeys.GetPackageKey(data.CycleType, req.StorePackageIndex, userSeq);
            if (packageKey != null)
            {
                var buyCountRedis = await redisUser.StringGetAsync(packageKey);
                refreshPackageInfo.BuyCount = (int)buyCountRedis;
                if (buyCountRedis.HasValue == true)
                {
                    if ((int)buyCountRedis >= data.PurchaseLimitCount)
                        return Ok<BuyPackageRes>(ResultCode.StoreLimit);
                    if (((int)buyCountRedis + req.BuyCount) > data.PurchaseLimitCount)
                        return Ok<BuyPackageRes>(ResultCode.StoreLimit);
                }

                // 첫 redis에 키 등록 되고 개인이 아닌 주기적 초기화 되는 제한일 경우 TTL 시간 적용
                if (buyCountRedis.HasValue == false && data.CycleType >= CycleType.Daily)
                    redisKeyExpire = true;
            }

            var usePackageResult = await _inventoryService.UseItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"buypackage",
                new List<ItemInfo> { new ItemInfo(data.PaymentType, data.PaymentIndex, data.PaymentAmount * req.BuyCount) });
            if (usePackageResult.resultCode != ResultCode.Success)
            {
                return Ok<BuyPackageRes>(usePackageResult.resultCode);
            }
            var rewardList = new List<ItemInfo>();

            var grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);
            var rewards = _playerService.GetCalcGoldItemInfos(grade, data.RewardInfos);
            for (int i = 0; i < req.BuyCount; i++)
                rewardList.AddRange(rewards);

            var rewardPackageResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"buypackage", rewardList);
            if (rewardPackageResult.resultCode != ResultCode.Success)
            {
                return Ok<BuyPackageRes>(rewardPackageResult.resultCode);
            }

            if (packageKey != null)
            {
                refreshPackageInfo.BuyCount = (int)await redisUser.StringIncrementAsync(packageKey, req.BuyCount);
                if (redisKeyExpire == true)
                    await redisUser.KeyExpireAsync(packageKey, AppClock.GetExprieCycleType((byte)data.CycleType));
            }

            var resp = new BuyPackageRes
            {
                RefreshInfo = usePackageResult.refreshItem.ToRefreshInventoryInfo().AddRefreshInfo(rewardPackageResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo()),
                RewardsInfo = rewardPackageResult.obtainResult.RewardsInfo,
                RefreshPackageInfo = refreshPackageInfo
            };

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();
            return Ok(ResultCode.Success, resp);
        }


        [HttpPost("buyinapp-check")]
        public async Task<ActionResult<BuyInAppCheckRes>> BuyInAppCheckAsync([FromBody] BuyInAppCheckReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<BuyInAppCheckRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var csvData = _csvContext.GetData();

            if (req.StoreProductType == StoreProductType.Package)
            {
                if (csvData.StorePackageDicData.TryGetValue(req.Index, out var data) == false)
                    return Ok<BuyInAppCheckRes>(ResultCode.InvalidParameter);

                if (data.View == false)
                    return Ok<BuyInAppCheckRes>(ResultCode.NotSellProduct);

                // packageKey 가 null 일 경우는 제한이 없는 경우 
                string packageKey = RedisKeys.GetPackageKey(data.CycleType, req.Index, userSeq);

                if (packageKey != null)
                {
                    var buyCountRedis = await redisUser.StringGetAsync(packageKey);
                    if (buyCountRedis.HasValue == true)
                    {
                        if ((int)buyCountRedis >= data.PurchaseLimitCount)
                            return Ok<BuyPackageRes>(ResultCode.StoreLimit);
                    }

                }
            }
            else if( req.StoreProductType == StoreProductType.PackageSpecial)
            {
               
            }    
           
            else
                return Ok<BuyInAppCheckRes>(ResultCode.InvalidParameter);

            return Ok(ResultCode.Success, new BuyInAppCheckRes
            {

            });
        }

        [HttpPost("buyinapp")]
        public async Task<ActionResult<BuyInAppRes>> BuyInAppAsync([FromBody] BuyInAppReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<BuyInAppRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var storeIndex = 0L;

            using var redLock = await _redLockFactory.CreateLockAsync(RedLockKeys.BuyIAPLock(req.PurchaseData.TransactionId), TimeSpan.FromSeconds(5));
            if (redLock.IsAcquired == false)
                return Ok<BuyInAppRes>(ResultCode.ReceiptAlreadyProcessed);

            var iapHistoryDb = await userCtx.IAPHistory.Where(p => p.TransactionId == req.PurchaseData.TransactionId).FirstOrDefaultAsync();
            if (iapHistoryDb != null)
                return Ok<BuyInAppRes>(ResultCode.ReceiptAlreadyProcessed);

            var validateResult = IapVerificationResult.Fail(false);

            var productInfo = new PackageStoreInfo();

            if (req.PurchaseData.StoreType == StoreType.GooglePlay)
            {
                validateResult = await _iapService.VerifyGooglePurchaseAsync(req.PurchaseData.Payload);
            }
            else if (req.PurchaseData.StoreType == StoreType.AppleAppStore)
            {
                validateResult = await _iapService.VerifyIosPurchaseAsync(req.PurchaseData.TransactionId, req.PurchaseData.Payload);
            }
            else
            {
                return Ok<BuyInAppRes>(ResultCode.InvalidParameter);
            }   

            if (validateResult.IsSuccess == false)
                return Ok<BuyInAppRes>(ResultCode.ReceiptValidationFailed);

            var rewardList = new List<ItemInfo>();
           
            if ( req.StoreProductType == StoreProductType.None)
            {
                req.StoreProductType = _inventoryService.GetStoreProductType(req.PurchaseData.StoreType, req.PurchaseData.ProductId);
                if (req.StoreProductType == StoreProductType.None)
                    return Ok<BuyInAppRes>(ResultCode.NotFound);
            }

            if( req.StoreProductType == StoreProductType.Package )
            {
                StorePackageCSVData packageData = null;
                if (req.PurchaseData.StoreType == StoreType.GooglePlay)
                    packageData = csvData.StorePackageDicData.Values.Where(p => p.AosProductId == req.PurchaseData.ProductId).FirstOrDefault();
                else if (req.PurchaseData.StoreType == StoreType.AppleAppStore)
                    packageData = csvData.StorePackageDicData.Values.Where(p => p.IosProductId == req.PurchaseData.ProductId).FirstOrDefault();

                if (packageData == null)
                    return Ok<BuyInAppRes>(ResultCode.NotFound);

                rewardList = packageData.RewardInfos;

                if (rewardList.Any() == false)
                    return Ok<BuyInAppRes>(ResultCode.NotFound);

                storeIndex = packageData.Index;
                string packageKey = RedisKeys.GetPackageKey(packageData.CycleType, packageData.Index, userSeq);
                if (packageKey != null)
                {
                    var buyCountRedis = await redisUser.StringGetAsync(packageKey);
                    await redisUser.StringIncrementAsync(packageKey);

                    // 첫 redis에 키 등록 되고 개인이 아닌 주기적 초기화 되는 제한일 경우 TTL 시간 적용
                    if (buyCountRedis.HasValue == false && packageData.CycleType >= CycleType.Daily)
                        await redisUser.KeyExpireAsync(packageKey, AppClock.GetExprieCycleType((byte)packageData.CycleType));

                }
            }
            else if( req.StoreProductType == StoreProductType.PackageSpecial)
            {
               
            }
            

            var rewardResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"buyinapp", rewardList);
            if (rewardResult.resultCode != ResultCode.Success)
                return Ok<BuyInAppRes>(rewardResult.resultCode);


            await userCtx.IAPHistory.AddAsync(new IAPHistoryModel
            {
                UserSeq = userSeq,
                StoreType = req.PurchaseData.StoreType,
                TransactionId = req.PurchaseData.TransactionId,
                ProductId = req.PurchaseData.ProductId,
//                ProductId = validateResult.ProductId,
                Receipt = req.PurchaseData.Payload,
            });

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            var buyInAppCount = await redisUser.StringIncrementAsync(string.Format(RedisKeys.s_UserBuyInAppCount, userSeq));

            return Ok(ResultCode.Success, new BuyInAppRes
            {
                StoreProductType = req.StoreProductType,
                StoreIndex = storeIndex,
                BuyInAppCount = buyInAppCount,
                RewardsInfo = rewardResult.obtainResult.RewardsInfo,
                RefreshInfo = rewardResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo()
            });
        }

    

    }
}


