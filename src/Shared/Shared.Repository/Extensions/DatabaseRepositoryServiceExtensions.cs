using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Repository.Database;
using Shared.Repository.Services;

namespace Shared.Repository.Extensions
{
    public static class DatabaseRepositoryServiceExtensions
    {
        public static void AddDatabaseRepositoryService(this IServiceCollection services,
            DatabaseRepositoryServiceOptions options)
        {
            services.AddSingleton(p => new DatabaseRepositoryService(
                p.GetRequiredService<ILoggerFactory>(), p,
                options));
        }
    }
}