using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class StoreGachaRewardGroupCSVData : BaseCSVData
    {
        public override string GetFileName() => "Store_Gacha_RewardGroup.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("fr_dt")]
        public DateTime FrDt { get; private set; }

        [CSVColumn("to_dt")]
        public DateTime ToDt { get; private set; }

        [CSVColumn("reward_group_id")]
        public long RewardGroupId { get; private set; }

        [CSVColumn("reward_type")]
        public RewardPaymentType RewardType { get; private set; }

        [CSVColumn("reward_index")]
        public long RewardIndex { get; private set; }

        [CSVColumn("reward_amount")]
        public BigInteger RewardAmount { get; private set; }

        [CSVColumn("prob_weight")]
        public long ProbWeight { get; private set; }

        [CSVColumn("payment_type")]
        public RewardPaymentType PaymentType { get; private set; }

        [CSVColumn("payment_index")]
        public long PaymentIndex { get; private set; }

        [CSVColumn("payment_amount")]
        public BigInteger PaymentAmount { get; private set; }

        public ItemInfo RewardInfo { get; private set; }
        public ItemInfo PaymentInfo { get; private set; }
        public bool IsValidTime(DateTime now)
        {
            return (FrDt <= now && now <= ToDt);
        }


        public override void Init()
        {
            base.Init();
            RewardInfo = new ItemInfo(RewardType, RewardIndex, RewardAmount);
            PaymentInfo = new ItemInfo(PaymentType, PaymentIndex, PaymentAmount);
        }

    }

}
