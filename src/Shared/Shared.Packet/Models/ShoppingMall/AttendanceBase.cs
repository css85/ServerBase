using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class AttendanceBase
    {   
        public int LastRewardDay { get; set; }                   // 마지막 받은 출석부 날짜 ( 몇일차 )
        public long AttendanceDtTick { get; set; }               // 마지막 출석 보상 받은 시간 
        public bool IsReward(DateTime currenctDt) => currenctDt.Date > new DateTime(AttendanceDtTick, DateTimeKind.Utc).Date;   // 보상 받을 수 있는지  
    }
}
