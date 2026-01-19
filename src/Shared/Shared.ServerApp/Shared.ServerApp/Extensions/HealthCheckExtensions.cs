using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Repository;
using Shared.Repository.Database;
using Shared.ServerApp.HealthCheck;

namespace Shared.ServerApp.Extensions
{
    public static class HealthCheckExtensions
    {
        public static void AddAppHealthCheck(this IServiceCollection services, DatabaseRepositoryServiceOptions dbRepoOptions)
        {
            var healthCheckerBuilder = services.AddHealthChecks()
                .AddCheck<RedisHealthCheck>("Redis-check", HealthStatus.Unhealthy, new[] { "redis" });

            if (dbRepoOptions.DatabaseOptions.Any(p => p.DbContextType == typeof(GateCtx)))
            {
                healthCheckerBuilder
                    .AddCheck<DbContextHealthCheck<GateCtx>>("GateDb-check", HealthStatus.Unhealthy, new[] { "db" });
            }            
            if (dbRepoOptions.DatabaseOptions.Any(p => p.DbContextType == typeof(UserCtx)))
            {
                healthCheckerBuilder
                    .AddCheck<DbContextHealthCheck<UserCtx>>("UserDb-check", HealthStatus.Unhealthy, new[] { "db", "eve" });
            }
            if (dbRepoOptions.DatabaseOptions.Any(p => p.DbContextType == typeof(StoreEventCtx)))
            {
                healthCheckerBuilder
                    .AddCheck<DbContextHealthCheck<StoreEventCtx>>("StoreEventDb-check", HealthStatus.Unhealthy, new[] { "db", "eve_store_event" });
            }

            services.AddRouting();
        }

        public static void UseAppHealthCheck(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc");
                endpoints.MapHealthChecks("/hc/detail", new HealthCheckOptions
                {
                    ResponseWriter = WriteResponse,
                });
            });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                {
                    var isDescription = string.IsNullOrEmpty(pair.Value.Description) == false;
                    var isData = pair.Value.Data != null && pair.Value.Data.Count > 0;
                    if (isDescription && isData)
                    {
                        return new JProperty(pair.Key, new JObject(
                            new JProperty("status", pair.Value.Status.ToString()),
                            new JProperty("description", pair.Value.Description),
                            new JProperty("data", new JObject(pair.Value.Data.Select(
                                p => new JProperty(p.Key, p.Value))))));
                    }
                    if (isDescription)
                    {
                        return new JProperty(pair.Key, new JObject(
                            new JProperty("status", pair.Value.Status.ToString()),
                            new JProperty("description", pair.Value.Description)));
                    }
                    if (isData)
                    {
                        return new JProperty(pair.Key, new JObject(
                            new JProperty("status", pair.Value.Status.ToString()),
                            new JProperty("data", new JObject(pair.Value.Data.Select(
                                p => new JProperty(p.Key, p.Value))))));
                    }
                    return new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString())));
                }))));

            return context.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }
    }
}