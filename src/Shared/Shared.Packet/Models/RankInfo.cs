using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class RankInfo
    {
        public int Rank { get; set; }
        public double Score { get; set; }
        public UserInfo UserInfo { get; set; } = new UserInfo();
    }   
}
