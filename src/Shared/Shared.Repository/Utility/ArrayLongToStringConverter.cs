using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace Shared.Repository.Utility
{
    public class ArrayLongToStringConverter : ValueConverter<long[], string>
    {
        public ArrayLongToStringConverter():base(
               v => 
                   string.Join(';', v),
                v => 
                    v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => long.Parse(x))
                        .ToArray())
        {
            
        }
    }
}
