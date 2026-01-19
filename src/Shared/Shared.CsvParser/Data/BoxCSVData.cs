using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class BoxCSVData : BaseCSVData
    {
        public override string GetFileName() => "Box.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("box_type")]
        public BoxType BoxType { get; private set; }

        [CSVColumn("selectbox_num")]
        public int SelectBoxNum { get; private set; }

        //[CSVColumn("open_qualification_type")]
        //public OpenQualificationType OpenQualificationType { get; private set; }

        [CSVColumn("open_qualification_value")]
        public long OpenQualificationValue { get; private set; }

        [CSVColumn("reward_group_id")]
        public long RewardGroupId { get; private set; }

        public List<BoxRewardGroupCSVData> RewardGroups { get; private set; } = new List<BoxRewardGroupCSVData>();
        public Dictionary<ItemInfo, long> RewardData { get; private set; }

    
        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            RewardGroups = csvData.BoxRewardGroupListData.Where(p => p.RewardGroupId == RewardGroupId).ToList();
            RewardData = RewardGroups.Select(p => KeyValuePair.Create(p.RewardInfo, p.ProbWeight)).ToDictionary(p => p.Key, p => p.Value);
        }

    }

}
