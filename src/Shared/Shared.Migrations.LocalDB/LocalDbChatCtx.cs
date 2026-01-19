using Microsoft.EntityFrameworkCore;
using Shared.Repository;
using Shared.Repository.Services;

namespace Shared.Migrations.LocalDB
{
    public sealed class LocalDbChatCtx : ChatCtx
    {
        public LocalDbChatCtx(DbContextOptions<LocalDbChatCtx> options,
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
                "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=audition_m_testing.chat;Trusted_Connection=True;",
            };
    }
}