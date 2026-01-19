using System;

namespace Shared.Clock
{
    public static class AppClock
    {
        public static TimeSpan _offset = TimeSpan.Zero;

        public static DateTime Now => DateTime.Now + _offset;
        public static DateTime UtcNow => DateTime.UtcNow + _offset;
        public static DateTimeOffset OffsetNow => DateTimeOffset.Now + _offset;
        public static DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow + _offset;

        public static void SetOffset(TimeSpan offset)
        {
            _offset = offset;
        }
    }
}
