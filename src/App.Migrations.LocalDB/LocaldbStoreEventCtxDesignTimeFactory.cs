using Microsoft.EntityFrameworkCore.Design;
using Shared.Repository;
using Shared.Repository.Extensions;
using Shared.ServerApp.Extensions;

namespace App.Migrations.MySQL
{
    public class MysqlStoreEventCtxFactory : IDesignTimeDbContextFactory<StoreEventCtx>
    {
        public StoreEventCtx CreateDbContext(string[] args)
        {
            var optionsBuilder =
                DesignTimeDbContextFactoryExtensions.DbContextOptionsBuilder<StoreEventCtx>(args,
                    p => p.StoreEventConnectionStrings)
                    .UseProvider(DatabaseProviderType.LocalDB)
                    .UseShardIndex(0);

            return new StoreEventCtx(optionsBuilder.Options);
        }
    }
}