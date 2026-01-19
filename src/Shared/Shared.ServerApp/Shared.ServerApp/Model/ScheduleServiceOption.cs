using System;

namespace Shared.ServerApp.Model
{
    public class ScheduleServiceOption
    {
        public NetServiceType[] TargetServiceTypes;

        public DateTime StartTime;
        public TimeSpan Interval;
        public bool IsSingleInstance;
    }
}
