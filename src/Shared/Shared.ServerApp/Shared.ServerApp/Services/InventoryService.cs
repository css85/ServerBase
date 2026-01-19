using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Nest;
using RedLockNet.SERedis;
using Shared;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.ServerApp.Services
{
    public class InventoryService
    {
        private readonly ILogger<InventoryService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedLockFactory _redLockFactory;
        private readonly UserContextDataService _userCtxService;
        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;
        private readonly MissionService _missionService;

        public InventoryService(
            ILogger<InventoryService> logger,
            DatabaseRepositoryService dbRepo,
            RedLockFactory redLockFactory,
            UserContextDataService userCtxService,
            CsvStoreContext csvStoreContext,
            MissionService missionService, 
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _redLockFactory = redLockFactory;
            _userCtxService = userCtxService;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;
            _missionService = missionService;

        }

        private ResultCode GetResultCodeItem(ItemInfo item)
        {
            var csvData = _csvContext.GetData();

            //if (item.ItemType == ObtainType.Item_Consume_Line)
            //    return ResultCode.NotHaveLine;

            //else if (item.ItemType == ObtainType.Item_Material)
            //{
            //    var materialType = csvData.ItemMaterialDicData[item.Index].ItemMaterialType;
            //    if (materialType == ItemMaterialType.Stage_Licenses)
            //        return ResultCode.NotExistsLicenseItem;
            //    else if (materialType == ItemMaterialType.Auction_Ticket)
            //        return ResultCode.NotEnoughAuctionTicket;
            //}
            return ResultCode.NotEnoughItem;
        }
        private ResultCode GetResultCodeCurrecy(CurrencyType currencyType)
        {
            switch(currencyType)
            {
                case CurrencyType.Ruby:         return ResultCode.NotEnoughRuby;
            }

            return ResultCode.NotEnoughCurrency;
        }

        private ResultCode GetResultCodePoint(PointType pointType)
        {
            //switch(pointType)
            //{
            //    case PointType.CrewPoint: return ResultCode.NotEnoughCrewPoint;
                   
            //}
            return ResultCode.NotEnoughPoint;
        }
        
        private async Task AddCurrencyHistoryAsync(UserCtx userCtx, long userSeq, BigInteger beforeQty, bool isUse, ItemInfo changeItemInfo, BigInteger afterQty, string reason)
        {
            await userCtx.CurrencyHistory.AddAsync(new CurrencyHistoryModel
            {
                UserSeq = userSeq,
                ItemType = changeItemInfo.ItemType,
                ItemId = changeItemInfo.Index,
                BeforeQty = beforeQty,
                ChangeQty = isUse ? -changeItemInfo.ItemQty : changeItemInfo.ItemQty,
                AfterQty = afterQty,
                Reason = reason,
            });

            //if (changeItemInfo.Index == 3)
            //{
            //    await userCtx.CurrencyGoldHistory.AddAsync(new CurrencyGoldHistoryModel
            //    {
            //        UserSeq = userSeq,
            //        ItemType = changeItemInfo.ItemType,
            //        ItemId = changeItemInfo.Index,
            //        BeforeQty = beforeQty,
            //        ChangeQty = isUse ? -changeItemInfo.ItemQty : changeItemInfo.ItemQty,
            //        AfterQty = afterQty,
            //        Reason = reason,
            //    });
            //}
            //else if (changeItemInfo.Index == 4)
            //{
            //    await userCtx.CurrencyInAppHistory.AddAsync(new CurrencyInAppHistoryModel
            //    {
            //        UserSeq = userSeq,
            //        ItemType = changeItemInfo.ItemType,
            //        ItemId = changeItemInfo.Index,
            //        BeforeQty = beforeQty,
            //        ChangeQty = isUse ? -changeItemInfo.ItemQty : changeItemInfo.ItemQty,
            //        AfterQty = afterQty,
            //        Reason = reason,
            //    });
            //}
        }

        public async Task<ResultCode> CheckItemCurrencyAsync(UserCtx userCtx, StoreEventCtx storeEventCtx, long userSeq, List<ItemInfo> useList)
        {
            var csvData = _csvContext.GetData();

            if (useList.Any() == false)
                return ResultCode.Success;

            //var groupUseList = useList.GroupBy(x => new { type = x.ItemType, id = x.Index })
            //    .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Sum(e => e.ItemQty) })
            //    .ToList();


            var groupUseList = useList.GroupBy(x => new { type = x.ItemType, id = x.Index })
                .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Aggregate<ItemInfo, BigInteger >(0,(value1, value2) => value1 + value2.ItemQty ) })
                .ToList();

            foreach (var item in groupUseList)
            {                
                switch (item.ItemType)
                {
                    case RewardPaymentType.Currency:
                        var currencyType = (CurrencyType)item.Index;

                        switch (currencyType)
                        {
                            case CurrencyType.Free:
                            case CurrencyType.Gold:
                            case CurrencyType.Heart:

                                break;

                            default:

                                var currencyDb = await userCtx.UserCurrency.FindAsync(userSeq, RewardPaymentType.Currency, item.Index);
                                if (currencyDb == null)
                                    return GetResultCodeCurrecy(currencyType);

                                if ((currencyDb.ItemQty - item.ItemQty) < 0)
                                    return GetResultCodeCurrecy(currencyType);
                                break;
                        }
                        break;


                    case RewardPaymentType.Material:
                    case RewardPaymentType.Box:
                        var itemDb = await userCtx.UserItems.FindAsync(userSeq, item.ItemType, item.Index);
                        if (itemDb == null)
                            return GetResultCodeItem(item);

                        if ((itemDb.ItemQty - item.ItemQty) < 0)
                            return GetResultCodeItem(item);
                        break;

                    case RewardPaymentType.Point:
                        var pointData = csvData.PointListData.Where(p => p.Index == item.Index).FirstOrDefault();

                        var pointType = pointData != null ? pointData.PointType : PointType.None;

                        var pointDb = await userCtx.UserPoints.FindAsync(userSeq, item.ItemType, item.Index);
                        if (pointDb == null)
                            return GetResultCodePoint(pointType);

                        if ((pointDb.ItemQty - item.ItemQty) < 0)
                            return GetResultCodePoint(pointType);
                        break;
                }
            }
            return ResultCode.Success;
        }

        public async Task<(ResultCode resultCode, RefreshGameItem refreshItem)> UseItemCurrencyAsync(UserCtx userCtx, StoreEventCtx storeEventCtx, long userSeq, string reason, List<ItemInfo> useList)
        {
            var refreshInfo = new RefreshGameItem();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            if (useList.Any() == false)
                return (ResultCode.Success, refreshInfo);

            //var groupUseList = useList.GroupBy(x => new { type = x.ItemType, id = x.Index })
            //    .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Sum(e => e.ItemQty) })
            //    .ToList();

            var groupUseList = useList.GroupBy(x => new { type = x.ItemType, id = x.Index })
                .Select(t => new ItemInfo ( t.Key.type, t.Key.id, t.Aggregate<ItemInfo, BigInteger>(0, (value1, value2) => value1 + value2.ItemQty) ))
                .ToList();

            var decManagePoint = 0L;

            foreach (var item in groupUseList)
            {
                BigInteger beforeQty = 0;
                switch (item.ItemType)
                {
                    case RewardPaymentType.Currency:
                        var currencyType = (CurrencyType)item.Index;

                        switch (currencyType)
                        {
                            case CurrencyType.Free:
                            case CurrencyType.Gold:
                            case CurrencyType.Heart:

                                break;

                            default:

                                var currencyDb = await userCtx.UserCurrency.FindAsync(userSeq, RewardPaymentType.Currency, item.Index);
                                if (currencyDb == null)
                                    return (GetResultCodeCurrecy(currencyType), null);

                                beforeQty = currencyDb.ItemQty;                                

                                if ((currencyDb.ItemQty - item.ItemQty) < 0)
                                    return (GetResultCodeCurrecy(currencyType), null);

                                await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Consume, Cond2Type.Currency, item.Index, item.ItemQty);

                                currencyDb.ItemQty -= (long)item.ItemQty;
                                userCtx.UserCurrency.Update(currencyDb);
                                await AddCurrencyHistoryAsync(userCtx, userSeq, beforeQty, true, item, currencyDb.ItemQty, reason);

                                item.ItemQty = currencyDb.ItemQty;
                                refreshInfo.RefreshItems.Add(item);

                                break;
                        }
                        break;
                        

                    case RewardPaymentType.Material:
                    case RewardPaymentType.Box:
                        var itemDb = await userCtx.UserItems.FindAsync(userSeq, item.ItemType, item.Index);
                        if (itemDb == null)
                            return (GetResultCodeItem(item), null);

                        beforeQty = itemDb.ItemQty;

                        if ((itemDb.ItemQty - item.ItemQty) < 0)
                            return (GetResultCodeItem(item), null);

                        if(item.ItemType == RewardPaymentType.Material)
                            await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Consume, Cond2Type.Material, item.Index, item.ItemQty);
                        
                        itemDb.ItemQty -= (long)item.ItemQty;

                        if (itemDb.ItemQty <= 0)
                            userCtx.UserItems.Remove(itemDb);
                        else
                            userCtx.UserItems.Update(itemDb);

                        await userCtx.ItemHistory.AddAsync(new ItemHistoryModel
                        {
                            UserSeq = userSeq,
                            ItemType = item.ItemType,
                            ItemId = item.Index,
                            BeforeQty = (long)beforeQty,
                            ChangeQty = -(long)item.ItemQty,
                            AfterQty = itemDb.ItemQty,
                            Reason = reason
                        });


                        item.ItemQty = itemDb.ItemQty;
                        refreshInfo.RefreshItems.Add(item);
                        break;

                    case RewardPaymentType.Point:

                        var pointData = csvData.PointListData.Where(p => p.Index == item.Index).FirstOrDefault();

                        var pointType = pointData != null ? pointData.PointType : PointType.None;

                        var pointDb = await userCtx.UserPoints.FindAsync(userSeq, item.ItemType, item.Index);
                        if (pointDb == null)
                            return (GetResultCodePoint(pointType), null);

                        beforeQty = pointDb.ItemQty;

                        if ((pointDb.ItemQty - item.ItemQty) < 0)
                            return (GetResultCodePoint(pointType), null);

                        pointDb.ItemQty -= (long)item.ItemQty;
                        userCtx.UserPoints.Update(pointDb);

                        await userCtx.PointHistory.AddAsync(new PointHistoryModel
                        {
                            UserSeq = userSeq,
                            ItemType = item.ItemType,
                            ItemId = item.Index,
                            BeforeQty = (long)beforeQty,
                            ChangeQty = -(long)item.ItemQty,
                            AfterQty = pointDb.ItemQty,
                            Reason = reason
                        });

                        if (pointType == PointType.ShoppingmallManage)
                            decManagePoint += (long)item.ItemQty;

                        item.ItemQty = pointDb.ItemQty;
                        refreshInfo.RefreshItems.Add(item);

                        break;

                }
            }
            if (decManagePoint > 0)
            {
                var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);

                double timestamp = (DateTimeOffset.MaxValue.ToUnixTimeSeconds() - AppClock.OffsetUtcNow.ToUnixTimeSeconds()) / 1000000000000d;
                var updateScore = await redisRank.SortedSetDecrementAsync(RedisKeys.ss_ShoppingmallRank, userSeq, decManagePoint);
                await redisRank.SortedSetAddAsync(RedisKeys.ss_ShoppingmallRank, userSeq, Math.Truncate(updateScore) + timestamp);
            }


            return (ResultCode.Success, refreshInfo);
        }

        
    
        public async Task<(ResultCode resultCode, ObtainResult obtainResult)> ObtainItemCurrencyAsync(UserCtx userCtx, StoreEventCtx storeEventCtx, long userSeq, string reason, List<ItemInfo> rewardList)
        {
            var obtainResult = new ObtainResult();
            var csvData = _csvContext.GetData();

            if( rewardList.Any() == false) 
                return (ResultCode.Success, obtainResult);

            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            // event box 가 있을 경우 풀어서 넣어준다 .
            //var eventBoxs = rewardList.Where(p => p.ItemType == ObtainType.Item_Event_Box).ToList();
            //if (eventBoxs.Any())
            //{
            //    rewardList.AddRange(GetEventBoxRewards(eventBoxs));
            //    rewardList.RemoveAll(p => p.ItemType == ObtainType.Item_Event_Box);
            //}

            obtainResult.RewardsInfo.Rewards = rewardList;

            var checkRewardResult = await CheckRewardDuplicateAsync(userCtx, userSeq, rewardList);
            obtainResult.RewardsInfo.DuplicateItemInfos = checkRewardResult.duplicateItemInfos;
            var groupRewardList = checkRewardResult.finalRewards;

//            await CheckEventAsync(userCtx, storeEventCtx, userSeq, groupRewardList);
            var incManagePoint = 0L;

            foreach (var item in groupRewardList)
            {
                BigInteger beforeQty = 0;
                switch (item.ItemType)
                {
                    case RewardPaymentType.Currency:
                        var currencyType = (CurrencyType)item.Index;

                        var currencyDb = await userCtx.UserCurrency.FindAsync(userSeq, RewardPaymentType.Currency, item.Index);
                        if (currencyDb == null)
                            return (GetResultCodeCurrecy(currencyType), null);

                        beforeQty = currencyDb.ItemQty;
                        currencyDb.ItemQty += item.ItemQty;

                        userCtx.UserCurrency.Update(currencyDb);
                        await AddCurrencyHistoryAsync(userCtx, userSeq, beforeQty, false, item, currencyDb.ItemQty, reason);

                        if(currencyType == CurrencyType.Gold || currencyType == CurrencyType.Heart)
                        {
                            obtainResult.RefreshInfo.RefreshItems.Add(item);
                        }
                        else
                        {
                            item.ItemQty = currencyDb.ItemQty;
                            obtainResult.RefreshInfo.RefreshItems.Add(item);
                        }

                        break;

                    
                    case RewardPaymentType.Material:
                    case RewardPaymentType.Box:
                        var itemDb = await userCtx.UserItems.FindAsync(userSeq, item.ItemType, item.Index);

                        if (itemDb == null)
                        {
                            itemDb = new UserGameItemOwnModel
                            {
                                UserSeq = userSeq,
                                ObtainType = item.ItemType,
                                ItemId = item.Index,
                                ItemQty = (long)item.ItemQty,
                            };
                            await userCtx.UserItems.AddAsync(itemDb);
                        }
                        else
                        {
                            beforeQty = itemDb.ItemQty;
                            itemDb.ItemQty += (long)item.ItemQty;
                            userCtx.UserItems.Update(itemDb);
                        }

                        await userCtx.ItemHistory.AddAsync(new ItemHistoryModel
                        {
                            UserSeq = userSeq,
                            ItemType = item.ItemType,
                            ItemId = item.Index,
                            BeforeQty = (long)beforeQty,
                            ChangeQty = (long)item.ItemQty,
                            AfterQty = itemDb.ItemQty,
                            Reason = reason
                        });

                        item.ItemQty = itemDb.ItemQty;
                        obtainResult.RefreshInfo.RefreshItems.Add(item);

                        break;
                    case RewardPaymentType.Point:
                        var pointData = csvData.PointListData.Where(p => p.Index == item.Index).FirstOrDefault();

                        var pointType = pointData != null ? pointData.PointType : PointType.None;

                        
                        if (pointType == PointType.VipPoint)
                        {
                            var userVipDb = await userCtx.UserVips.Where(p => p.UserSeq == userSeq).FirstOrDefaultAsync();

                            if (userVipDb == null)
                                return (GetResultCodePoint(pointType), null);

                            var beforeLevel = userVipDb.Level;
                            var beforePoint = userVipDb.Point;

                            var maxLevel = csvData.VipListData.Max(p => p.VipLevel);
                            if( userVipDb.Level >= maxLevel )
                            {
                                userVipDb.Point += (int)item.ItemQty;
                            }
                            else
                            {
                                userVipDb.Point += (int)item.ItemQty;
                                for( int i = userVipDb.Level; i < maxLevel; i++ )
                                {
                                    var calcLevelData = csvData.VipListData.Where(p => p.VipLevel == i).FirstOrDefault();
                                    if (userVipDb.Point >= calcLevelData.VipLevelupPoint)
                                    {
                                        userVipDb.Level = i + 1;
                                        userVipDb.Point -= calcLevelData.VipLevelupPoint;
                                    }
                                    else
                                        break;
                                }
                            }
                            userCtx.UserVips.Update(userVipDb); 
                            obtainResult.RefreshInfo.VipBase =  userVipDb.ToVipBase();

                            await userCtx.VipHistory.AddAsync(new VipHistoryModel
                            {
                                UserSeq = userSeq,
                                BeforeLevel = beforeLevel,
                                BeforePoint = beforePoint,
                                ChangePoint = (int)item.ItemQty,
                                AfterLevel = userVipDb.Level,
                                AfterPoint = userVipDb.Point,
                                Reason = reason
                            });

                            if (beforeLevel != userVipDb.Level)
                                await redisUser.HashSetAsync(string.Format(RedisKeys.hs_UserVip, userSeq), "level", userVipDb.Level);
                        }                    
                        else
                        {
                            var pointDb = await userCtx.UserPoints.FindAsync(userSeq, item.ItemType, item.Index);
                            if (pointDb == null)
                                return (GetResultCodePoint(pointType), null);

                            beforeQty = pointDb.ItemQty;
                            pointDb.ItemQty += (long)item.ItemQty;
                            userCtx.UserPoints.Update(pointDb);

                            await userCtx.PointHistory.AddAsync(new PointHistoryModel
                            {
                                UserSeq = userSeq,
                                ItemType = item.ItemType,
                                ItemId = item.Index,
                                BeforeQty = (long)beforeQty,
                                ChangeQty = (long)item.ItemQty,
                                AfterQty = pointDb.ItemQty,
                                Reason = reason
                            });

                            if (pointType == PointType.ShoppingmallManage)
                            {
                                incManagePoint += (long)item.ItemQty;
                            }


                            item.ItemQty = pointDb.ItemQty;
                            obtainResult.RefreshInfo.RefreshItems.Add(item);
                        }
                        
                        break;

                }
            }
            obtainResult.RefreshInfo.RefreshItems = obtainResult.RefreshInfo.RefreshItems.GroupBy(x => new { type = x.ItemType, id = x.Index })
                .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Max(e => e.ItemQty) })
                .ToList();


            if (incManagePoint > 0)
            {
                var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);

                double timestamp = (DateTimeOffset.MaxValue.ToUnixTimeSeconds() - AppClock.OffsetUtcNow.ToUnixTimeSeconds()) / 1000000000000d;
                var updateScore = await redisRank.SortedSetIncrementAsync(RedisKeys.ss_ShoppingmallRank, userSeq, incManagePoint);
                await redisRank.SortedSetAddAsync(RedisKeys.ss_ShoppingmallRank, userSeq, Math.Truncate(updateScore) + timestamp);

            }

            return (ResultCode.Success, obtainResult);
        }

        public async Task<(List<ItemInfo> finalRewards, List<DuplicateItemInfo> duplicateItemInfos)> CheckRewardDuplicateAsync(UserCtx userCtx, long userSeq, List<ItemInfo> rewardList)
        {
            var csvData = _csvContext.GetData();
            var finalRewardList = new List<ItemInfo>();
            var duplicateItemInfoList = new List<DuplicateItemInfo>();

            //var groupRewardList = rewardList.GroupBy(x => new { type = x.ItemType, id = x.Index })
            //    .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Sum(e => e.ItemQty) })
            //    .ToList();

            var groupRewardList = rewardList.GroupBy(x => new { type = x.ItemType, id = x.Index })
                .Select(t => new ItemInfo { ItemType = t.Key.type, Index = t.Key.id, ItemQty = t.Aggregate<ItemInfo, BigInteger>(0, (value1, value2) => value1 + value2.ItemQty) })
                .ToList();

            //var etcPartsDbs = new List<UserPartsModel>();
            //var partsList = groupRewardList.Where(p => p.ItemType == RewardPaymentType.Parts).ToList();
            //if (partsList.Any())
            //{
            //    var etcPartsIndexList = csvData.PartsPurchaseEtcListData.Select(p => p.PartsIndex).ToList();
            //    if (etcPartsIndexList.Any() == false)
            //    {
            //        return (groupRewardList, duplicateItemInfoList);
            //    }
            //    etcPartsDbs = await userCtx.UserParts.Where(p => p.UserSeq == userSeq && etcPartsIndexList.Contains(p.PartsIndex)).ToListAsync();
            //}
            //else
            {
                return (groupRewardList, duplicateItemInfoList);
            }

            //foreach (var item in groupRewardList)
            //{
            //    if (item.ItemType == RewardPaymentType.Parts)
            //    {
            //        var partsEtcData = csvData.PartsPurchaseEtcListData.Where(p => p.PartsIndex == item.Index).FirstOrDefault();
            //        if (partsEtcData != null)
            //        {
            //            if (etcPartsDbs.Exists(p => p.PartsIndex == item.Index))
            //            {
            //                duplicateItemInfoList.Add(new DuplicateItemInfo
            //                {
            //                    OrigineItemInfo = item,
            //                    ReplaceItemInfo = partsEtcData.ReturnRewardInfo,

            //                });

            //                finalRewardList.Add(new ItemInfo(partsEtcData.ReturnRewardType, partsEtcData.ReturnRewardIndex, partsEtcData.ReturnRewardAmount * item.ItemQty));
            //            }
            //            else
            //            {
            //                if (item.ItemQty == 1)
            //                {
            //                    finalRewardList.Add(item);
            //                }
            //                else if (item.ItemQty > 1)
            //                {
            //                    duplicateItemInfoList.Add(new DuplicateItemInfo
            //                    {
            //                        OrigineItemInfo = new ItemInfo(item.ItemType, item.Index, item.ItemQty - 1),
            //                        ReplaceItemInfo = partsEtcData.ReturnRewardInfo,
            //                    });
            //                    finalRewardList.Add(new ItemInfo(partsEtcData.ReturnRewardType, partsEtcData.ReturnRewardIndex, partsEtcData.ReturnRewardAmount * (item.ItemQty - 1)));
            //                    finalRewardList.Add(new ItemInfo(item.ItemType, item.Index, 1));
            //                }

            //            }
            //        }
            //        else
            //            finalRewardList.Add(item);
            //    }
            //    else
            //        finalRewardList.Add(item);

            //}

            //return (finalRewardList, duplicateItemInfoList);
        }
        
       

        
        // 인앱상풍 테이블 추가 될때 마다 추가 필요 
        public StoreProductType GetStoreProductType(StoreType storeType, string productId)
        {
            var csvData = _csvContext.GetData();

            if (storeType == StoreType.GooglePlay)
            {
                if (csvData.StorePackageDicData.Values.Any(p => p.AosProductId == productId))
                    return StoreProductType.Package;
                else if (csvData.StorePackageSpecialDicData.Values.Any(p => p.AosProductId == productId))
                    return StoreProductType.PackageSpecial;
            }
            else if (storeType == StoreType.AppleAppStore)
            {
                if (csvData.StorePackageDicData.Values.Any(p => p.IosProductId == productId))
                    return StoreProductType.Package;
                else if (csvData.StorePackageSpecialDicData.Values.Any(p => p.IosProductId == productId))
                    return StoreProductType.PackageSpecial;
            }

            return StoreProductType.None;
        }

    }
}
