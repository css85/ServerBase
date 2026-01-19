using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class VipCSVData : BaseCSVData
    {
        public override string GetFileName() => "VIP.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("vip_level")]
        public int VipLevel { get; private set; }

        [CSVColumn("vip_levelup_point")]
        public int VipLevelupPoint { get; private set; }

        [CSVColumn("vipbuff_group_id")]
        public long VipBuffGroupId { get; private set; }

        [CSVColumn("dailyreward_group_id")]
        public long DailyRewardGroupId { get; private set; }


        public List<VipBuffListGroupCSVData> BuffGroups { get; private set; } = new List<VipBuffListGroupCSVData>();
        public List<VipDailyRewardGroupCSVData> DailyRewardGroups { get; private set; } = new List<VipDailyRewardGroupCSVData>();
        public List<ItemInfo> DailyRewardInfos { get; private set; } = new List<ItemInfo>();

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);
            BuffGroups = csvData.VipBuffListGroupListData.Where(p=>p.VipBuffGroupId == VipBuffGroupId).ToList();
            DailyRewardGroups = csvData.VipDailyRewardGroupListData.Where(p => p.DailyRewardGroupId == DailyRewardGroupId).ToList();
            DailyRewardInfos = DailyRewardGroups.Select(p=>p.RewardInfo).ToList();
        }

    }

}
