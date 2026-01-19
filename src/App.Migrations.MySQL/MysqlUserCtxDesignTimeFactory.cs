using Microsoft.EntityFrameworkCore.Design;
using Shared.Repository;
using Shared.Repository.Extensions;
using Shared.ServerApp.Extensions;

namespace App.Migrations.MySQL
{
    public class UserCtxFactory : IDesignTimeDbContextFactory<UserCtx>
    {
        public UserCtx CreateDbContext(string[] args)
        {
            var optionsBuilder =
                DesignTimeDbContextFactoryExtensions.DbContextOptionsBuilder<UserCtx>(args,
                    p => p.UserConnectionStrings)
                    .UseProvider(DatabaseProviderType.MySQL)
                    .UseShardIndex(0);

            return new UserCtx(optionsBuilder.Options);
        }
    }
}