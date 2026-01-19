
using Shared.Packet.Models;
using Shared.ServerModel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Shared.ServerApp.Utility
{
    public static class ObtainItemExtensions
    {
        public static RefreshInventoryInfo ToRefreshInventoryInfo(this RefreshGameItem info)
        {
            var refreshInfo = new RefreshInventoryInfo();

            refreshInfo.CurrencyList.AddRange(info.RefreshItems.Where(x => x.ItemType == RewardPaymentType.Currency)
                .GroupBy(x => x.Index)
                .Select(g =>
                    new CurrencyInfo
                    {
                        Currency = (CurrencyType)g.Key,
                        Amount = g.Aggregate<ItemInfo, BigInteger>(0, (value1, value2) => value1 + value2.ItemQty)
//                        Amount = g.Sum(x => x.ItemQty)
                    }));

            refreshInfo.ItemList.AddRange(info.RefreshItems.Where(x =>
                x.ItemType is
                RewardPaymentType.Material or 
                RewardPaymentType.Box)
                .ToList());

            refreshInfo.PointList.AddRange(info.RefreshItems.Where(x =>
                x.ItemType is
                RewardPaymentType.Point )
                .ToList());

            

            refreshInfo.VipBase = info.VipBase;

            return refreshInfo;

        }
        //public static RefreshInventoryInfo ToRefreshInventoryInfo(this List<ItemInfo> itemList)
        //{
        //    var refreshInfo = new RefreshInventoryInfo();

        //    refreshInfo.CurrencyList.AddRange(itemList.Where(x => x.ItemType == RewardPaymentType.Currency)
        //        .GroupBy(x=> x.Index)
        //        .Select(g => 
        //            new CurrencyInfo
        //            {
        //                Currency = (CurrencyType)g.Key, 
        //                Amount = g.Sum(x=>x.ItemQty)
        //            }));

        //    refreshInfo.ItemList.AddRange(itemList.Where(x =>
        //        x.ItemType is
        //        RewardPaymentType.Material or
        //        RewardPaymentType.Parts)

        //        .ToList());


        //    return refreshInfo;
        //}

        


        public static RefreshInventoryInfo AddRefreshInfo(this RefreshInventoryInfo info_1, RefreshInventoryInfo info_2)
        {
            info_1.CurrencyList.AddRange(info_2.CurrencyList);
            info_1.ItemList.AddRange(info_2.ItemList);
            info_1.PointList.AddRange(info_2.PointList);
            return info_1;
        }
    }
}
