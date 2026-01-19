using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shared.Repository.Options;

namespace Shared.Repository.Extensions;

public static class DbContextShardIndexOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseShardIndex(
        this DbContextOptionsBuilder optionsBuilder,
        int shardIndex)
    {
        var alreadyExtension = optionsBuilder.Options.FindExtension<DbContextShardIndexOptions>();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
            (alreadyExtension != null)
                ? alreadyExtension.WithShardIndex(shardIndex)
                : new DbContextShardIndexOptions().WithShardIndex(shardIndex));

        return optionsBuilder;
    }
}