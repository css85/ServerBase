using Microsoft.EntityFrameworkCore;
using Shared.Repository;
using Shared.Repository.Services;

namespace Shared.Migrations.MySQL
{
    public sealed class MysqlGateCtx : GateCtx
    {
        public MysqlGateCtx(DbContextOptions<MysqlGateCtx> options,
            DatabaseRepositoryService dbRepo,
            int index) : base(options, dbRepo, index, DatabaseProviderType.MySQL)
        {
            Database.AutoTransactionsEnabled = false;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}