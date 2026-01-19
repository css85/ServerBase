using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Shared.Repository.Database;
using Shared.Repository.Extensions;

namespace Shared.Repository.Services
{
    public class DatabaseRepositoryService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<DatabaseRepositoryService> _logger;

        private readonly DatabaseRepositoryServiceOptions _databaseRepositoryServiceOptions;
        private readonly DatabaseProviderType _databaseProviderType;

        private readonly Dictionary<Type, Func<IPooledDbContext>[]> _dbContextCreatorsMap = new();

        public const int TempShardKey = 0;

        public DatabaseProviderType DatabaseProviderType => _databaseProviderType;

        public DatabaseRepositoryService(
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            DatabaseRepositoryServiceOptions databaseRepositoryServiceOptions
        )
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<DatabaseRepositoryService>();
            _databaseRepositoryServiceOptions = databaseRepositoryServiceOptions;

            _databaseProviderType = serviceProvider.GetRequiredService<IConfiguration>().GetDatabaseProviderType();

            foreach (var databaseShardOption in databaseRepositoryServiceOptions.DatabaseOptions)
                RegisterDb(databaseShardOption);
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        public int GetShardCount<T>()
        {
            return _dbContextCreatorsMap.TryGetValue(typeof(T), out var values) ? values.Length : 0;
        }

        private void RegisterDb(DatabaseOption option)
        {
            if (option.ConnectionStrings == null ||
                option.ConnectionStrings.Length == 0)
            {
                throw new Exception($"Empty ConnectionStrings: {option.DbContextType}");
            }

            var queues = new ConcurrentQueue<PooledDbContext>[option.ShardCount];
            for (var i = 0; i < option.ShardCount; i++)
                queues[i] = new ConcurrentQueue<PooledDbContext>();

            var waitQueues = new ConcurrentQueue<TaskCompletionSource<IPooledDbContext>>[option.ShardCount];
            for (var i = 0; i < option.ShardCount; i++)
                waitQueues[i] = new ConcurrentQueue<TaskCompletionSource<IPooledDbContext>>();

            var dbContextCreators = new Func<IPooledDbContext>[option.ConnectionStrings.Length];
            for (var i = 0; i < option.ConnectionStrings.Length; i++)
            {
                var connectionString = option.ConnectionStrings[i];
                if (string.IsNullOrEmpty(connectionString))
                    continue;

                var dbContextOptionsBuilderType = typeof(DbContextOptionsBuilder<>);
                dbContextOptionsBuilderType = dbContextOptionsBuilderType.MakeGenericType(option.DbContextType);

                var dbContextOptionsBuilder =
                    (DbContextOptionsBuilder) Activator.CreateInstance(dbContextOptionsBuilderType);

                if (option.EnableQueryLogging)
                    dbContextOptionsBuilder!.UseLoggerFactory(_loggerFactory);

                dbContextOptionsBuilder!
                    .EnableSensitiveDataLogging(option.EnableQueryLogging)
                    .EnableDetailedErrors(option.EnableQueryLogging);

                dbContextOptionsBuilder
                    .UseProvider(_databaseProviderType)
                    .UseShardIndex(i);

                try
                {
                    switch (_databaseProviderType)
                    {
                        case DatabaseProviderType.MySQL:
                        {
                            dbContextOptionsBuilder.UseMySql(connectionString, ServerVersion.Parse("8.0",ServerType.MySql),
                                p =>
                                    p.MigrationsAssembly("App.Migrations.MySQL"));
                            break;
                        }
                        case DatabaseProviderType.LocalDB:
                        {
                            dbContextOptionsBuilder.UseSqlServer(connectionString,
                                p => p.MigrationsAssembly("App.Migrations.LocalDB"));
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "DbContext create failed");
                    throw;
                }

                var dbContextFactoryType = typeof(PooledDbContextFactory<>);
                dbContextFactoryType = dbContextFactoryType.MakeGenericType(option.DbContextType);

                var dbContextOptionsProperty = dbContextOptionsBuilder.GetType()
                    .GetProperties().FirstOrDefault(p => p.Name == nameof(dbContextOptionsBuilder.Options) && p.DeclaringType == dbContextOptionsBuilderType);
                var dbContextOptions = dbContextOptionsProperty!.GetValue(dbContextOptionsBuilder);

                var dbContextFactory =
                    Activator.CreateInstance(dbContextFactoryType, dbContextOptions, 1024);

                var createDbContextMethod = dbContextFactoryType.GetMethod("CreateDbContext");
                dbContextCreators[i] = () =>
                {
                    try
                    {
                        return (IPooledDbContext) createDbContextMethod!.Invoke(dbContextFactory, null);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, "DbContext create failed");
                        return null;
                    }
                };
            }
            _dbContextCreatorsMap.Add(option.DbContextType, dbContextCreators);
        }

        public GateCtx GetGateDb()
        {
            return GetDb<GateCtx>(0);
        }

        public StoreEventCtx GetStoreEventDb()
        {
            return GetDb<StoreEventCtx>(0);
        }

        
        public UserCtx GetUserDb()
        {
            return GetDb<UserCtx>(0);
        }

        public UserCtx GetUserDb(long seq)
        {
            var shardKey = DatabaseSequenceExtensions.ExtractShardIndex(seq);
            return GetDb<UserCtx>(shardKey);
        }

        public AllPooledDbContext<T> GetAllDb<T>() where T : PooledDbContext
        {
            return new AllPooledDbContext<T>(this);
        }

        public T GetDb<T>(int index) where T : PooledDbContext
        {
            return GetDb<T>(typeof(T), index);
        }
        private T GetDb<T>(Type type, int index) where T : PooledDbContext
        {
            var dbContext = (T)_dbContextCreatorsMap[type][index].Invoke();

            return dbContext;
        }
    }
}