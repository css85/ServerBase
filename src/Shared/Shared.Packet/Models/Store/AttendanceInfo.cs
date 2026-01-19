using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class AttendanceInfo : AttendanceBase
    {
        public long Index { get; set; }                          // 출석부 인덱스
        public AttendanceType AttendanceType { get; set; }       // 출석부 타입
        public int AttendNotifyLocalHour { get; set; }           // 출석부 로컬 푸쉬 알림 로컬 시스템 시간
    }
}
