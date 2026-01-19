using Shared.Entities.Models;
using Shared.Packet.Models;

namespace WebTool.Base.UserInfoDetail
{
    public class PlayerDetailInfo
    {
        public readonly long UserSeq;

        public AccountModel Account { get; set; }
        public UserInfoModel User { get; set; }
        
        public bool IsBlock { get; set; }

        public PlayerDetailInfo(long userSeq)
        {
            UserSeq = userSeq;
        }
    }
}
