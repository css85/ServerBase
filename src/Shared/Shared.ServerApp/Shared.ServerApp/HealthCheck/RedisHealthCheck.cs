using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Shared.Services.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.ServerApp.HealthCheck
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly RedisRepositoryService _redisRepo;

        public RedisHealthCheck(RedisRepositoryService redisRepo)
        {
            _redisRepo = redisRepo;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await _redisRepo.App.StringGetAsync("health_check");
            }
            catch (DbException e)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "", e);
            }

            return HealthCheckResult.Healthy();
        }
    }
}