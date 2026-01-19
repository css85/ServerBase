using Microsoft.EntityFrameworkCore.Design;
using Shared.Repository;
using Shared.Repository.Extensions;
using Shared.ServerApp.Extensions;

namespace App.Migrations.MySQL
{
    public class GateCtxFactory : IDesignTimeDbContextFactory<GateCtx>
    {
        public GateCtx CreateDbContext(string[] args)
        {
            var optionsBuilder =
                DesignTimeDbContextFactoryExtensions.DbContextOptionsBuilder<GateCtx>(args,
                    p => p.GateConnectionStrings)
                    .UseProvider(DatabaseProviderType.MySQL)
                    .UseShardIndex(0);

            return new GateCtx(optionsBuilder.Options);
        }
    }
}