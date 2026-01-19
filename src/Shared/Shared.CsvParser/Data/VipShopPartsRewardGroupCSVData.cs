using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class VipShopPartsRewardGroupCSVData : BaseCSVData
    {
        public override string GetFileName() => "VIP_Shop_Parts_RewardGroup.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("product_group_id")]
        public long ProductGroupId { get; private set; }

        [CSVColumn("product_order")]
        public int ProductOrder { get; private set; }

        [CSVColumn("badge")]
        public StoreBadgeType Badge { get; private set; }

        [CSVColumn("fr_viplevel")]
        public int FrVipLevel { get; private set; }

        [CSVColumn("to_viplevel")]
        public int ToVipLevel { get; private set; }

        [CSVColumn("reward_type")]
        public RewardPaymentType RewardType { get; private set; }

        [CSVColumn("reward_index")]
        public long RewardIndex { get; private set; }

        [CSVColumn("reward_amount")]
        public BigInteger RewardAmount { get; private set; }

        [CSVColumn("payment_type")]
        public RewardPaymentType PaymentType { get; private set; }

        [CSVColumn("payment_index")]
        public long PaymentIndex { get; private set; }

        [CSVColumn("payment_amount")]
        public BigInteger PaymentAmount { get; private set; }

        public bool IsValidVipLevel(int level) => FrVipLevel <= level && level <= ToVipLevel;
        public bool IsFree => PaymentType == RewardPaymentType.Currency && PaymentIndex == (long)CurrencyType.Free;
        public ItemInfo RewardInfo { get; private set; }
        public ItemInfo PaymentItemInfo { get; private set; }


        

        public override void Init()
        {
            base.Init();
            RewardInfo = new ItemInfo(RewardType, RewardIndex, RewardAmount);
            PaymentItemInfo = new ItemInfo(PaymentType, PaymentIndex, PaymentAmount);
        }

    }

}
