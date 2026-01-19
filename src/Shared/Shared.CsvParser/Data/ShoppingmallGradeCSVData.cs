using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class ShoppingmallGradeCSVData : BaseCSVData
    {
        public override string GetFileName() => "ShoppingMall_Grade.csv";

        [CSVColumn("index")]
        public long Index { get; private set; }

        [CSVColumn("shoppingmall_grade", primaryKey: true)]
        public int ShoppingmallGrade { get; private set; }

        [CSVColumn("unlock_shoppingmall_level")]
        public int UnlockShoppingmallLevel { get; private set; }

        [CSVColumn("grade_required_gold")]
        public BigInteger GradeRequiredGold { get; private set; }

        [CSVColumn("reward_group_id")]
        public long RewardGroupId { get; private set; }

        [CSVColumn("offlinegift_reward_group_id")]
        public long OfflineRewardGroupId { get; private set; }

        [CSVColumn("fairy_reward_gold_value")]
        public BigInteger FairyRewardGoldValue { get; private set; }

        [CSVColumn("reward_gold_value")]
        public BigInteger RewardGoldValue { get; private set; }

        [CSVColumn("contest_gold_buff")]
        public int ContestGoldBuff { get; private set; }

        [CSVColumn("profile_posting_max")]
        public int ProfilePostingMax { get; private set; }

        [CSVColumn("sponsorship_mission_max")]
        public int SponsorshipMissionMax { get; private set; }

        [CSVColumn("leaflet_charging_speed")]
        public int LeafletChargingSpeed { get; private set; }

        [CSVColumn("leaflet_max")]
        public int LeafletMax { get; private set; }

        public List<ItemInfo> RewardDatas { get; private set; } = new List<ItemInfo>();


        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);
            RewardDatas = csvData.ShoppingmallGradeRewardGroupListData.Where(p => p.RewardGroupId == RewardGroupId).Select(p => p.RewardInfo).ToList();   
        }

    }

}
