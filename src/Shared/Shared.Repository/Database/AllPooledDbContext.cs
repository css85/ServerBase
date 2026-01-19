using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Repository.Extensions;
using Shared.Repository.Services;

namespace Shared.Repository.Database
{
    public class AllPooledDbContext<T> : IDisposable where T : PooledDbContext
    {
        public static readonly IReadOnlySet<T> Empty = new HashSet<T>();

        private readonly DatabaseRepositoryService _dbRepo;

        private bool IsActive => _spawnedDbContexts != null;
        private T[] _spawnedDbContexts;

        public T this[int index] => Get(index);
        public int Length => _spawnedDbContexts.Length;

        public AllPooledDbContext(DatabaseRepositoryService dbRepo)
        {
            _dbRepo = dbRepo; 
            _spawnedDbContexts = new T[_dbRepo.GetShardCount<T>()];
        }

        public T Get(long seq)
        {
            var shardIndex = DatabaseSequenceExtensions.ExtractShardIndex(seq);
            return Get(shardIndex);
        }

        public IReadOnlySet<T> GetMultipleAsync([NotNull]IEnumerable<long> seqList)
        {
            HashSet<T> shardList = null;
            foreach (var seq in seqList)
            {
                shardList ??= new HashSet<T>(_spawnedDbContexts.Length);
                shardList.Add(Get(seq));
            }
            return shardList??Empty;
        }

        private T Get(int shardIndex)
        {
            if (shardIndex < 0 || shardIndex >= _spawnedDbContexts.Length)
                shardIndex = Math.Abs(shardIndex) % _spawnedDbContexts.Length;

            if (_spawnedDbContexts[shardIndex] != null)
                return _spawnedDbContexts[shardIndex];

            var dbContext = _dbRepo.GetDb<T>(shardIndex);
            _spawnedDbContexts[shardIndex] = dbContext;

            return dbContext;
        }

        public async ValueTask SaveChangesAsync()
        {
            foreach (var dbContext in _spawnedDbContexts)
            {
                if (dbContext != null)
                    await dbContext.SaveChangesAsync();
            }
        }

        public void ClearChangeTrackers()
        {
            foreach (var dbContext in _spawnedDbContexts)
            {
                if (dbContext != null)
                    dbContext.ChangeTracker.Clear();
            }
        }

        public void Dispose()
        {
            if (IsActive)
            {
                foreach (var dbContext in _spawnedDbContexts)
                    dbContext?.Dispose();

                _spawnedDbContexts = null;
            }
        }

        ~AllPooledDbContext()
        {
            if (IsActive)
                _dbRepo.GetLogger().LogWarning("AllActiveDbContext is not disposed");
        }
    }
}