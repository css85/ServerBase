using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class StorePackageCSVData : BaseCSVData
    {
        public override string GetFileName() => "Store_Package.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("view")]
        public bool View { get; private set; }

        [CSVColumn("category")]
        public string Category { get; private set; }

        [CSVColumn("limit_count_cycle")]
        public CycleType CycleType { get; private set; }

        [CSVColumn("purchase_limit_count")]
        public int PurchaseLimitCount { get; private set; }

        [CSVColumn("aos_store_product_id")]
        public string AosProductId { get; private set; }

        [CSVColumn("ios_store_product_id")]
        public string IosProductId { get; private set; }


        [CSVColumn("payment_type")]
        public RewardPaymentType PaymentType { get; private set; }

        [CSVColumn("payment_index")]
        public long PaymentIndex { get; private set; }

        [CSVColumn("payment_amount")]
        public BigInteger PaymentAmount { get; private set; }

        [CSVColumn("payment_discount_rate")]
        public long PaymentDiscountRate { get; private set; }

        [CSVColumn("reward_group_id")] 
        public long RewardGroupId { get; private set; }

        
        public List<ItemInfo> RewardInfos { get; private set; } = new List<ItemInfo>();
        public bool IsFree { get; private set; }

        public override void Init()
        {
            base.Init();
            IsFree = (PaymentType == RewardPaymentType.Currency && PaymentIndex == (long)CurrencyType.Free);
//            PaymentAmount = Math.Truncate( PaymentAmount * (1 - (PaymentDicountRate * 0.01M)) );
            PaymentAmount = PaymentAmount * (100 - PaymentDiscountRate) / 100;
        }
        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            RewardInfos = csvData.StorePackageRewardGroupListData.Where(p => p.RewardGroupId == RewardGroupId).Select(p=>p.RewardInfo).ToList();  
        }

        
        
    }
}
