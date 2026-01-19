using System;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class MissionBase
    {
        public long MissionIndex { get; set; }       // 일일미션 : index, 업적 : group_index, 네비게이션: index
        public BigInteger CurrentCount { get; set; }        // 현재까지 누적 카운트        

        public MissionBase() { }
        public MissionBase(long missionIndex) 
        {
            this.MissionIndex = missionIndex;
            this.CurrentCount = 0;
        }

    }
}
