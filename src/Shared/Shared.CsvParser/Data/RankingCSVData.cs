using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class RankingCSVData : BaseCSVData
    {
        public override string GetFileName() => "Ranking.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("rank_from")]
        public int RankFrom { get; private set; }

        [CSVColumn("rank_to")]
        public int RankTo { get; private set; }

        [CSVColumn("reward_group_id")]
        public int RewardGroupId { get; private set; }

        public List<ItemInfo> RewardInfos { get; private set; } = new List<ItemInfo>();

        public bool IsValidRank(int rank) => RankFrom <= rank && RankTo >= rank;

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);
            RewardInfos = csvData.RankingRewardGroupListData.Where(p=>p.RewardGroupId == RewardGroupId).Select(p=>p.RewardInfo).ToList();  
        }
    }

}
