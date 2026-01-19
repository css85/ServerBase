using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class ProfileInfo
    {   
        public List<PartsBase> Parts { get; set; } = new List<PartsBase>();
        public int LikeBestRank { get; set; } = 0;    

        public void Clear()
        {
            Parts = new List<PartsBase>();
            LikeBestRank = 0;
        }

        public ProfileInfo CopyTo()
        {
            return new ProfileInfo
            {
                Parts = Parts,
                LikeBestRank = LikeBestRank
            };
        }
    }
}
