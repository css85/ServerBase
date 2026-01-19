using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Repository.Options;

namespace Shared.Repository.Database
{
    public class PooledDbContext : DbContext, IPooledDbContext
    {
        public readonly DatabaseProviderType Provider;

        public Type Type { get; }
        public int ShardIndex { get; }

        public PooledDbContext(DbContextOptions options) : base(options)
        {
            Type = GetType();

            var dbContextProviderOptions = options.GetExtension<DbContextProviderOptions>();
            Provider = dbContextProviderOptions.Provider;

            var dbContextShardOptions = options.GetExtension<DbContextShardIndexOptions>();
            ShardIndex = dbContextShardOptions.ShardIndex;
        }

        public bool IsNotEqualShard(PooledDbContext dbContext)
        {
            return ShardIndex != dbContext.ShardIndex;
        }

        public bool IsNotEqualShard(int shardIndex)
        {
            return ShardIndex != shardIndex;
        }

        public void Init()
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }
}