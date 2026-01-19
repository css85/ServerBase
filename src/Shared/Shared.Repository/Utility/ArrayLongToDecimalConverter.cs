using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Shared.Repository.Utility
{
    public class ArrayLongToDecimalConverter : ValueConverter<long[], string>
    {
        public ArrayLongToDecimalConverter():base(
               v => 
                   string.Join(';', v.Select(x => x.ToString())),
                v => 
                    v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => long.Parse(x))
                        .ToArray())
        {
        }
    }

    public class ArrayLongToDecimalComparer : ValueComparer<long[]>
    {
        public ArrayLongToDecimalComparer() : base((c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray())
        {
        }
    }
}
