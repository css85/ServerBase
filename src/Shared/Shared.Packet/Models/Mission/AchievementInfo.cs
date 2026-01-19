using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class AchievementInfo : MissionBase
    {   
        public int LastRewardOrderNum { get; set; }  // 최종 보상받은 ordernum
        public AchievementInfo() { }    
        public AchievementInfo(long index)
        {
            LastRewardOrderNum = 0;
            MissionIndex = index;
            CurrentCount = 0;
        }
    }
}
