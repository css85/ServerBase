using System;

namespace Shared.ServerApp.Config
{
    public class ClockSettings
    {
        public TimeSpan Offset { get; set; }
        public TimeSpan RenewalOffset { get; set; }    // 갱신시간 UTC+
        public int AttendNotifyLocalHour { get; set; } // 출석부 로컬 푸쉬 알림 로컬 시스템 시간
    }
}
