using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Buffers.Text;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace Shared.Repository.Utility
{
    public class ArrayLongToHexConverter : ValueConverter<long[], string>
    {
        public ArrayLongToHexConverter():base(
               v => 
                   string.Join(';', v.Select(x => x.ToString("X"))),
                v => 
                    v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => long.Parse(x,NumberStyles.HexNumber))
                        .ToArray())
        {
        }
    }

    public class ArrayLongToHexComparer : ValueComparer<long[]>
    {
        public ArrayLongToHexComparer() : base((c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray())
        {
        }
    }
}
