using System;
using System.Globalization;

namespace Shared.Clock
{
    public static class AppClock 
    {
        private static TimeSpan _offset = TimeSpan.Zero;
//        private static TimeSpan _localUtcOffset = TimeSpan.Zero;
        private static TimeSpan _renewalOffset = TimeSpan.Zero;
        private static int _attendNotifyLocalHour = 20;

#if RELEASE
        public static DateTime Now => DateTime.Now;
        public static DateTime UtcNow => DateTime.UtcNow;
        public static DateTime Today => DateTime.Today;
        public static DateTime UtcToday => DateTime.UtcNow.Date;

        public static DateTimeOffset OffsetNow => DateTimeOffset.Now;
        public static DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
#else
        public static DateTime Now => DateTime.Now + _offset;
        public static DateTime UtcNow => DateTime.UtcNow + _offset;
        public static DateTime Today => (DateTime.Now + _offset).Date;
        public static DateTime UtcToday => (DateTime.UtcNow + _offset).Date;

        public static DateTimeOffset OffsetNow => DateTimeOffset.Now + _offset;
        public static DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow + _offset;
#endif
        public static DateTime RenewalTime => GetRenewalTime(UtcNow);
        public static DateTime PrevRenewalTime => GetPrevRenewalTime(UtcNow);
        public static DateTime NextRenewalTime => GetNextRenewalTime(UtcNow);

        public static DateTimeOffset OffsetUtcToday => (DateTimeOffset.UtcNow + _offset).Date;
        public static DateTimeOffset OffsetRenewalTime => OffsetUtcNow + _renewalOffset;

        public static DateTime MaxValue => new DateTime(9999, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime MinValue => new DateTime(1999, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime AbuseMaxValue => new DateTime(9000, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        public static TimeSpan RenewalOffset => _renewalOffset;
        public static int AttendNotifyLocalHour => _attendNotifyLocalHour;

        public const long SecondTicks = 10000000L;
        public const long MinuteTicks = 60L * SecondTicks;
        public const long HourTicks = 60L * MinuteTicks;
        public const long DayTicks = 24L * HourTicks;
        public const long Day15Ticks = DayTicks * 15;
        public const long Day14Ticks = DayTicks * 14;
        public const long WeekTicks = DayTicks * 7;

        public static void SetOffset(TimeSpan offset)
        {
            _offset = offset;
        }

        public static void AddOffset(TimeSpan offset)
        {
            _offset += offset;
        }

        public static void SetRenewalOffset(TimeSpan offset)
        {
            _renewalOffset = offset;
        }

        public static void SetAttendNotifyLocalHour(int hour)
        {
            _attendNotifyLocalHour = hour;
        }

        public static DayOfWeek GetWeek(DateTime time)
        {
            return CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            //            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            var day = GetWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static string GetWeekString(DateTime time)
        {
            int week = GetIso8601WeekOfYear(time);

            int year = week >= 52 & time.Month == 1 ? time.Year - 1 : time.Year;
            return string.Format("{0}/{1}", year, week);
        }
        public static string GetMonthString(DateTime time)
        {
            return string.Format("{0}-{1}", time.Year, time.Month);
            
        }

        public static DateTime GetRenewalTime(DateTime now)
        {
            return now + _renewalOffset;
        }

        public static DateTime GetPrevRenewalTime(DateTime now)
        {
            return GetNextRenewalTime(now).AddDays(-1);
        }

        public static DateTime GetNextRenewalTime(DateTime now)
        {
            return (now + _renewalOffset).Date + _renewalOffset;
        }
        public static TimeSpan GetExprieCycleType(byte cycleType)
        {
            if (cycleType == 2) // Daily
                return TimeSpan.FromDays(3);
            if (cycleType == 3) // Weekly
                return TimeSpan.FromDays(10);
            if (cycleType == 4) // Monthly
                return TimeSpan.FromDays(35);

            return TimeSpan.MaxValue;
        }

        public static TimeSpan GetExprieCycleTypeHiddenItem(byte cycleType)
        {
            if (cycleType == 2) // Daily
                return TimeSpan.FromDays(3);
            if (cycleType == 3) // Weekly
                return TimeSpan.FromDays(16);
            if (cycleType == 4) // Monthly
                return TimeSpan.FromDays(65);

            return TimeSpan.MaxValue;
        }
        public static TimeSpan GetExprieCycleTypeStoreAd(int cycleType)
        {
            if (cycleType == 2) // Daily
                return TimeSpan.FromDays(1);
            if (cycleType == 3) // Weekly
                return TimeSpan.FromDays(7);
            if (cycleType == 4) // Monthly
                return TimeSpan.FromDays(30);

            return TimeSpan.MaxValue;
        }

    }
}
