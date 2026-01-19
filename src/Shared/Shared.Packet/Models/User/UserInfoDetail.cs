using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class UserInfoDetail : UserSimple
    {
        public long RepresentPostingSeq { get; set; }
        public int ShoppingmallRank { get; set; }
        public long ManagePoint { get; set; }       
        public string Comment { get; set; }
        public long LatestConnectDtTick { get; set; }
        public ProfileInfo ProfileInfo { get; set; } = new ProfileInfo();        
    }
}
