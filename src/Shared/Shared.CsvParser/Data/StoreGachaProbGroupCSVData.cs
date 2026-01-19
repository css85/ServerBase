using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class StoreGachaProbGroupCSVData : BaseCSVData
    {
        public override string GetFileName() => "Store_gacha_ProbGroup.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("prob_group_id")]
        public long ProbGroupId { get; private set; }

        [CSVColumn("reward_group_id")]
        public long RewardGroupId { get; private set; }

        [CSVColumn("mileage_group_id")]
        public long MileageGroupId { get; private set; }

        [CSVColumn("prob_weight")]
        public long ProbWeight { get; private set; }

        public List<StoreGachaRewardGroupCSVData> RewardGroups { get; private set; } = new List<StoreGachaRewardGroupCSVData>();
//        public Dictionary<ItemInfo, long> RewardData { get; private set; }
        public List<StoreGachaMileageCSVData> RewardMileageGroups { get; private set; } = new List<StoreGachaMileageCSVData>();
        public Dictionary<ItemInfo, long> RewardMileageData { get; private set; }

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            RewardGroups = csvData.StoreGachaRewardGroupListData.Where(p => p.RewardGroupId == RewardGroupId).ToList();
//            RewardData = RewardGroups.Select(p => KeyValuePair.Create(p.RewardInfo, p.ProbWeight)).ToDictionary(p => p.Key, p => p.Value);

            //RewardMileageGroups = csvData.StoreGachaMileageListData.Where(p => p.MileageGroupId == MileageGroupId).ToList();
            //RewardMileageData = RewardMileageGroups.Select(p => KeyValuePair.Create(p.RewardInfo, p.ProbWeight)).ToDictionary(p => p.Key, p => p.Value);
        }
    }

}
