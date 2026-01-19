using Microsoft.EntityFrameworkCore;
using Shared.Repository;
using Shared.Repository.Services;

namespace Shared.Migrations.LocalDB
{
    public sealed class LocalDbAccountCtx : AccountCtx
    {
        public LocalDbAccountCtx(DbContextOptions<LocalDbAccountCtx> options,
            DatabaseRepositoryService dbRepo,
            int index) : base(options, dbRepo, index, DatabaseProviderType.LocalDB)
        {
            Database.AutoTransactionsEnabled = false;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public static string[] GetTestingConnectionStrings() =>
            new[]
            {
                "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=audition_m_testing.account;Trusted_Connection=True;",
            };
    }
}
