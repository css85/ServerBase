using System;

namespace Shared.Session.Settings
{
    public class SessionSettingsBase
    {
        public TimeSpan PingCheckDelay { get; set; }
        public TimeSpan PingRequiredTime { get; set; }
        public TimeSpan PingTimeout { get; set; }

        public TimeSpan SessionLogDelay { get; set; }

        public bool EnableTracePacketLog { get; set; }
        public TimeSpan SlowPacketTime { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}
