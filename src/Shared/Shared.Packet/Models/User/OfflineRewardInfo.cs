
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Packet.Models
{
    [Serializable]
    public class OfflineRewardInfo
    {
        public bool Rewarded { get; set; }
        public int OfflineTimeMin { get; set; }
        public List<ItemInfo> RewardInfos { get; set; } = new List<ItemInfo>();

        public bool IsReward => Rewarded == false && RewardInfos.Any();

        public OfflineRewardInfo() { }  
        public OfflineRewardInfo(bool rewarded, int offlineTimeMin, List<ItemInfo> rewardInfos)
        {
            Rewarded = rewarded;
            OfflineTimeMin = offlineTimeMin;
            RewardInfos = rewardInfos;
        }
    }
}
