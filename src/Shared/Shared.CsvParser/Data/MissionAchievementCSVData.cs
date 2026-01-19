using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class MissionAchievementCSVData : BaseCSVData
    {
        public override string GetFileName() => "Mission_Achievement.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("mission_group")]
        public long MissionGroupIndex { get; private set; }

        [CSVColumn("mission_order")]
        public int MissionOrder { get; private set; }

        [CSVColumn("cond1")]
        public Cond1Type Cond1 { get; private set; }

        [CSVColumn("cond2")]
        public Cond2Type Cond2 { get; private set; }

        [CSVColumn("cond3")]
        public string Cond3 { get; private set; }

        [CSVColumn("count")]
        public int Count { get; private set; }

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
