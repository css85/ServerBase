using Shared.CsvData;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Shared.Packet;
using Shared.ServerModel;
using Shared.Packet.Models;
using Shared.Clock;
using System;

namespace Shared.CsvParser.Extensions
{
    public static class CsvDataExtensions
    {
        public static PackageStoreInfo ToPackageStoreInfo(this StorePackageCSVData data, int buyCount)
        {
            return new PackageStoreInfo
            {
                Index = data.Index,
                BuyCount = buyCount,
                Limit = data.PurchaseLimitCount,
                LimitCountCycle = data.CycleType,
            };
        }

        public static GachaStoreInfo ToGachaStoreInfo(this StoreGachaCSVData data)
        {
            return new GachaStoreInfo
            {
                Index = data.Index,
                ToDtTick = data.ToDt.Ticks,
            };
        }

       

    }
}
