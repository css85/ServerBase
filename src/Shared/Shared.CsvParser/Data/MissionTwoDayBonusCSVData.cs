using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class MissionTwoDayBonusCSVData : BaseCSVData
    {
        public override string GetFileName() => "Mission_TwodayBonus.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("mission_dt")]
        public DateTime MissionDt { get; private set; }

        [CSVColumn("reward_type")]
        public RewardPaymentType RewardType { get; private set; }

        [CSVColumn("reward_index")]
        public long RewardIndex { get; private set; }

        [CSVColumn("reward_amount")]
        public BigInteger RewardAmount { get; private set; }

        public ItemInfo RewardInfo { get; private set; }

        public override void Init()
        {
            base.Init();
            RewardInfo = new ItemInfo(RewardType, RewardIndex, RewardAmount);
        }
    }

}
