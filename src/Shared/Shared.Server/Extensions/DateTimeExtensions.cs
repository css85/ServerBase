using System;

namespace Shared.Server.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetPastTime(this DateTime dateTime, TimeSpan time)
        {
            if (dateTime.TimeOfDay < time)
                return dateTime.Date + time;
            return dateTime.Date.AddDays(1) + time;
        }

        public static DateTime GetPastWeekDate(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            var date = dateTime.Date;
            return date.AddDays(-(int)date.DayOfWeek  + (date.DayOfWeek < dayOfWeek ? (int) dayOfWeek - 7 : (int) dayOfWeek));
        }

        public static DateTime GetPastMonthStartDate(this DateTime dateTime)
        {
            var previousMonthDate = dateTime.Date.AddMonths(-1);
            return new DateTime(previousMonthDate.Year, previousMonthDate.Month, 1);
        }

        public static DateTime GetPastMonthEndDate(this DateTime dateTime)
        {
            var date = dateTime.Date;
            return new DateTime(date.Year, date.Month, 1).AddDays(-1);
        }
    }
}