using System;
using System.Threading.Tasks;
using Shared.Services.Redis;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared.Repository;
using Shared.Repository.Extensions;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Extensions;

namespace Shared.ServerApp.Services
{
    public class SequenceService
    {
        private string[] _sequenceTypes =
        {
            RedisKeys.s_UserSeqCounter, 
            
        };

        private readonly ILogger<SequenceService> _logger;
        private readonly RedisRepositoryService _redisRepo;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedLockFactory _redLockFactory;
        public SequenceService(
            ILogger<SequenceService> logger,
            RedisRepositoryService redisRepo,
            DatabaseRepositoryService dbRepo,
            RedLockFactory redLockFactory
            )
        {
            _logger = logger;
            _redisRepo = redisRepo;
            _dbRepo = dbRepo;
            _redLockFactory = redLockFactory;
        }

        // 42byte + 11byte + 10byte
        private async Task<long> MakeSeqAsync(string name, int shardCount)
        {
            var uniqueValue = await _redisRepo.App.StringIncrementAsync(name) % 0x7FF;
            return DatabaseSequenceExtensions.MakeSeq(uniqueValue, shardCount);
        }

        public async Task<long> MakeIdAsync(string name, int max)
        {
            return await _redisRepo.App.StringIncrementAsync(name) % max;
        }

        public Task<long> MakeUserSeqAsync() =>
            MakeSeqAsync(RedisKeys.s_UserSeqCounter, _dbRepo.GetShardCount<UserCtx>());
        
        //private async Task LoadSequenceDataAsync()
        //{
        //    using var appCtx = _dbRepo.GetAppDb();
        //    using (var redLock = await _redLockFactory.CreateLockAsync(RedLockKeys.SequenceService))
        //    {
        //        if (redLock.IsAcquired == false)
        //            return;

        //        for (var i = 0; i < _sequenceTypes.Length; i++)
        //        {
        //            var name = _sequenceTypes[i];
                    
        //            var sequence = await appCtx.Sequences.FindAsync(name).ConfigureAwait(false);
        //            if (sequence == null)
        //            {
        //                continue;
        //            }
                    
        //            await _redisRepo.App.StringSetAsync(name,sequence.Sequence);
        //        }
        //    }
        //}
        //private async Task SaveSequenceDatAsync()
        //{
        //    using var appCtx = _dbRepo.GetAppDb();
        //    using (var redLock = await _redLockFactory.CreateLockAsync(RedLockKeys.SequenceService))
        //    {
        //        if (redLock.IsAcquired == false)
        //            return;

        //        for (var i = 0; i < _sequenceTypes.Length; i++)
        //        {
        //            var name = _sequenceTypes[i];
        //            var sequenceValue = await _redisRepo.App.StringGetAsync(name);
        //            if( !sequenceValue.HasValue ||  long.TryParse(sequenceValue, out var value)) continue;
                    
        //            var sequence = await appCtx.Sequences.FindAsync(name).ConfigureAwait(false);
        //            if (sequence == null)
        //            {
        //                await appCtx.Sequences.AddAsync(new SequenceInfoModel()
        //                {
        //                    Type = name,
        //                    Sequence = value
        //                });
        //                await appCtx.SaveChangesAsync();
        //                continue;
        //            }
                    
        //            appCtx.Sequences.Update(sequence);
        //            await appCtx.SaveChangesAsync();
        //        }
        //    }
        //}
    }
}