using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class DailyMissionBonusInfo
    {
        public int MissionCompleteCount { get; set; }
        public ItemInfo RewardInfo { get; set; } = new ItemInfo();
        public bool IsRewarded { get; set; }             // true : 보상 받음 false : 보상 안받음 

        public DailyMissionBonusInfo() { }
        public DailyMissionBonusInfo(ItemInfo reward)
        {
            MissionCompleteCount = 0;
            IsRewarded = false;
            RewardInfo = reward;
        }
        public DailyMissionBonusInfo(int count, ItemInfo reward, bool rewarded = false)
        {
            MissionCompleteCount = count;
            IsRewarded = rewarded;
            RewardInfo = reward;
        }
    }
}
