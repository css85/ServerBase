using Shared.CsvParser;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.CsvData
{
    public class CouponRewardGroupCSVData : BaseCSVData
    {
        public override string GetFileName() => "Coupon_RewardGroup.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("reward_group_id")]
        public int RewardGroupId { get; private set; }

        [CSVColumn("reward_type")]
        public RewardPaymentType RewardType { get; private set; }

        [CSVColumn("reward_index")]
        public long RewardIndex { get; private set; }

        [CSVColumn("reward_amount")]
        public int RewardAmount { get; private set; }

        public ItemInfo RewardInfo { get; set; }
        public override void Init()
        {
            base.Init();

            RewardInfo = new ItemInfo(RewardType, RewardIndex, RewardAmount);

        }
    }
}
