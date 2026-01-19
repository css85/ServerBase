using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class UserMail
    {
        public long MailSeq { get; set; }
        public string TitleKey { get; set; }
        public string[] TitleKeyArgs { get; set; }
        public ItemInfo rewardInfo { get; set; }
        public bool IsInfinity { get; set; }
        public long LimitDtTick { get; set; }
    }
}
