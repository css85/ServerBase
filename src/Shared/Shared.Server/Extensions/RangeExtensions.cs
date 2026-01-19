using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Server.Extensions
{
    public static class RangeExtensions
    {
        public static bool IsInRange(this int num, int min, int max, bool inclusive =true)
        {
            return inclusive ? min <= num && num <= max
                            : min < num && num < max;
        }
        
        public static bool IsInRange(this long num, long min, long max, bool inclusive =true)
        {
            return inclusive ? min <= num && num <= max
                : min < num && num < max;
        }
        
        public static bool IsInRange(this double num, double min, double max, bool inclusive =true)
        {
            return inclusive ? min <= num && num <= max
                : min < num && num < max;
        }

        public static bool IsInRange(this DateTime num, DateTime min, DateTime max, bool inclusive = true)
        {
            return inclusive ? min <= num && num <= max
                : min < num && num < max;
        }
    }

    public static class CollectionExtensions
    {
        public static IEnumerable<TResult> SortBy<TResult, TKey>(
            this IEnumerable<TResult> itemsToSort,
            IEnumerable<TKey> sortKeys,
            Func<TResult, TKey> matchFunc)
        {
            return sortKeys.Join(itemsToSort,
                key => key,
                matchFunc,
                (key, iitem) => iitem);
        }
    }
}

