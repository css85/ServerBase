using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Packet;
using Shared.Repository.Database;
using Shared.Repository.Extensions;

namespace Shared.Repository
{
    public sealed class StoreEventCtx : PooledDbContext
    {
        public StoreEventCtx(DbContextOptions options) : base(options)
        {
        }
        
        private string DecimalStringSplit(string s) => s.Split('.')[0];

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4", null);

            ValueComparer<List<long>> indexListComparer = new(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

            ValueConverter<List<long>, string> indexListConverter = new(
                strings => string.Join(";", strings),
                s => s.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList());

            ValueConverter<BigInteger, string> bigIntegerStringConverter = new(
                b => b.ToString(),
                d => BigInteger.Parse(DecimalStringSplit(d), System.Globalization.NumberStyles.Number));
            //                d => BigInteger.Parse( d.Split(".", StringSplitOptions.RemoveEmptyEntries)[0]));

            ValueConverter<BigInteger, long> bigIntegerlongConverter = new(
               b => (long)b,
               d => new BigInteger(d));

            
            base.OnModelCreating(modelBuilder);
        }


    }
}

