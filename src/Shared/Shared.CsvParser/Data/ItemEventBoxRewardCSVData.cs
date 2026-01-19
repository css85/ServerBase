
using Shared.CsvParser;
using Shared.Packet.Models;

namespace Shared.CsvData
{
    public class ItemEventBoxRewardCSVData : BaseCSVData
    {
        public override string GetFileName() => "Item_Event_Box_Reward.csv";

        [CSVColumn("Index", primaryKey: true)]
        public long Id { get; private set; }
        
        [CSVColumn("EventBox_Reward_Group_ID")]
        public long GroupId { get; private set; }
            
        [CSVColumn("Obtain_Type")]
        public RewardPaymentType ObtainType { get; private set; }

        public byte ObtainTypeByte => (byte)ObtainType;

        [CSVColumn("Obtain_Index")]
        public long ObtainIndex { get; private set; }

        [CSVColumn("Obtain_Qty")]
        public int ObtainQty { get; private set; }

        [CSVColumn("Prob_Weight")]
        public long ProbWeight { get; private set; }

        public ItemInfo RewardInfo { get; private set; }

        public override void Init()
        {
            base.Init();

            RewardInfo = new ItemInfo(ObtainType, ObtainIndex, ObtainQty);

        }
    }
}
