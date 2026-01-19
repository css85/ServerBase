using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class StoreGachaPickupCSVData : BaseCSVData
    {
        public override string GetFileName() => "Store_Gacha_Pickup.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("fr_dt")]
        public DateTime FrDt { get; private set; }

        [CSVColumn("to_dt")]
        public DateTime ToDt { get; private set; }

        [CSVColumn("gacha_index")]
        public long GachaIndex { get; private set; }

        [CSVColumn("set_id")]
        public string SetId { get; private set; }

        [CSVColumn("prob_group_id")]
        public long ProbGroupId { get; private set; }

        public bool IsValidTime(DateTime now) => FrDt <= now && now < ToDt;

        public List<StoreGachaProbGroupCSVData> ProbGroups { get; private set; } = new List<StoreGachaProbGroupCSVData>();
        public Dictionary<StoreGachaProbGroupCSVData, long> ProbGroupData { get; private set; }


        public override void Init()
        {
            base.Init();
       
            var setId = long.Parse(SetId);
            SetId = $"{setId:D4}";
        }

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            ProbGroups = csvData.StoreGachaProbGroupListData.Where(p => p.ProbGroupId == ProbGroupId).ToList();
            ProbGroupData = ProbGroups.Select(p => KeyValuePair.Create(p, p.ProbWeight)).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
