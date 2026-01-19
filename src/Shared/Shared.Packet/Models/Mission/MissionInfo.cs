using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class MissionInfo : MissionBase
    {   
        public long ToDtTick { get; set; }
        public int MaxCount { get; set; }
        public bool IsReward { get; set; } = false;

        public bool ActiveReward()
        {
            if (IsReward == true)
                return false;

            if (CurrentCount >= MaxCount)
                return true;

            return false;
        }

        public double MissionRate()
        {
            if (CurrentCount <= 0)
                return 0d;
            var rate = Math.Min((double)((double)CurrentCount / (double)MaxCount * 100d), 100d);
            return rate;
            
        }
    }
}
