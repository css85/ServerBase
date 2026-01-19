using Shared.CsvParser;
using Shared.Packet;
using Shared.Packet.Models;
using Shared.ServerApp.Services;
using Shared.ServerModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.CsvData
{
    public class CouponCSVData : BaseCSVData
    {
        public override string GetFileName() => "Coupon.csv";

        [CSVColumn("coupon_code", primaryKey: true)]
        public string CouponCode { get; private set; }

        [CSVColumn("reward_give_type")]
        public CouponType CouponType { get; private set; }

        [CSVColumn("reward_give_num")]
        public int RewardGiveNum { get; private set; }

        [CSVColumn("reward_group_id")]
        public long RewardGroupId { get; private set; }

        [CSVColumn("fr_dt")]
        public DateTime FrDt { get; private set; }

        [CSVColumn("to_dt")]
        public DateTime ToDt { get; private set; }

        public List<ItemInfo> rewardInfoList { get; private set; }

        public bool IsValidTime(DateTime now)
        {
            return (FrDt <= now && now < ToDt);
        }


        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);
            rewardInfoList = csvData.CouponRewardListData
                .Where(p => p.RewardGroupId == RewardGroupId)
                .Select(p => p.RewardInfo)
                .ToList();
        }
    }
}
