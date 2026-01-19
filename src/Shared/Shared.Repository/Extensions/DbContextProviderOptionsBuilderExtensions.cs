using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shared.Repository.Options;

namespace Shared.Repository.Extensions;

public static class DbContextProviderOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseProvider(
        this DbContextOptionsBuilder optionsBuilder,
        DatabaseProviderType provider)
    {
        var alreadyExtension = optionsBuilder.Options.FindExtension<DbContextProviderOptions>();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
            (alreadyExtension != null)
                ? alreadyExtension.WithProvider(provider)
                : new DbContextProviderOptions().WithProvider(provider));

        return optionsBuilder;
    }
}