using System;
using System.Collections.Generic;
using Shared.Packet;
using Shared.Server.Define;

namespace Shared.Server.Extensions
{
 
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T value)
        {
            yield return value;
        }
    }

    public static class NetServiceTypeExtensions
    {
        public static string GetName(this NetServiceType serviceType)
        {
            return serviceType.ToString();
        }
    }

    public static class PeriodTypeExtensions
    {
       
    }
}
