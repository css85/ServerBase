using Microsoft.EntityFrameworkCore;
using Shared.Repository;
using Shared.Repository.Services;

namespace Shared.Migrations.LocalDB
{
    public sealed class LocalDbAppCtx : AppCtx
    {
        public LocalDbAppCtx(DbContextOptions<LocalDbAppCtx> options,
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
                "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=audition_m_testing.app;Trusted_Connection=True;",
            };
    }
}