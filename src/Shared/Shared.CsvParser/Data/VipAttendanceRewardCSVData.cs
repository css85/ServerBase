using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class VipAttendanceRewardCSVData : BaseCSVData
    {
        public override string GetFileName() => "VIP_Attendance_Reward.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("day_num")]
        public int DayNum { get; private set; }

        [CSVColumn("reward_type")]
        public RewardPaymentType RewardType { get; private set; }

        [CSVColumn("reward_index")]
        public long RewardIndex { get; private set; }

        [CSVColumn("reward_amount")]
        public int RewardAmount { get; private set; }

        public ItemInfo RewardInfo { get; private set; }

        public override void Init()
        {
            base.Init();
            RewardInfo = new ItemInfo(RewardType, RewardIndex, RewardAmount);
        }

    }

}
