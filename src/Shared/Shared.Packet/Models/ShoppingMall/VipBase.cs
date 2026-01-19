using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class VipBase
    {
        public int Level { get; set; } = -1;             // VIP 레벨
        public int Point { get; set; }                   // VIP 포인트
        public long VipBenefitEndDtTick { get; set; }    // VIP 활성화 끝나는 시간

        public bool IsActiveVip(long currenctDtTick) => currenctDtTick < VipBenefitEndDtTick;
    }
}
