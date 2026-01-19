using System;
using Shared.Clock;

namespace SampleGame.Shared.Extensions
{
    public static class DefaultTypeConverterExtenstions
    {
        public static bool IsNone(this DateTime utcTime)
        {
            return utcTime == AppClock.MinValue;
        }

        public static bool IsNone(this DateTimeOffset utcTime)
        {
            return utcTime == AppClock.MinValue;
        }

        public static bool IsInfinity(this DateTime utcTime)
        {
            return utcTime >= AppClock.MaxValue;
        }

        public static bool IsInfinity(this DateTimeOffset utcTime)
        {
            return utcTime >= AppClock.MaxValue;
        }


    }
}
