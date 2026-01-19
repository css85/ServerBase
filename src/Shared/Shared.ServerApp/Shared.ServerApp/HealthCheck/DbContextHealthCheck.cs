using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Repository.Database;
using Shared.Repository.Services;

namespace Shared.ServerApp.HealthCheck
{
    public class DbContextHealthCheck<T> : IHealthCheck where T : PooledDbContext
    {
        private readonly DatabaseRepositoryService _dbRepo;
        public DbContextHealthCheck(DatabaseRepositoryService dbRepo)
        {
            _dbRepo = dbRepo;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var shardCount = _dbRepo.GetShardCount<T>();
            for (var i = 0; i < shardCount; i++)
            {
                try
                {
                    using var dbCtx = _dbRepo.GetDb<T>(i);
                    await dbCtx.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                }
                catch (DbException e)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, $"index: {i}", e, new Dictionary<string, object>
                    {
                        {"index", i},
                    });
                }
            }

            return HealthCheckResult.Healthy();
        }
    }
}