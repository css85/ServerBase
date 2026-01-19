using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Shared.Server.Define;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
#pragma warning disable 1998

namespace Benchmark
{
    [MemoryDiagnoser]
    public class TestRedis
    {
        private readonly IRedisCacheConnectionPoolManager _redisPool;

        private const int RedisWarmUpCount = 10;
        private const int IterationCount = 100;

        public TestRedis()
        {
            ServiceUtility.Initialize();

            _redisPool = ServiceUtility.ServiceProvider.GetRequiredService<IRedisCacheConnectionPoolManager>();

            var redisWarmTasks = new Task[RedisWarmUpCount];
            foreach (var type in Enum.GetValues<RedisDatabase>())
            {
                if (type != RedisDatabase.None)
                {
                    for (var i = 0; i < RedisWarmUpCount; i++)
                        redisWarmTasks[i] = _redisPool.GetDb(type).PingAsync();
                }
            }

            Task.WhenAll(redisWarmTasks).Wait();
        }

        [Benchmark]
        public async Task TestNormal1Async()
        {
            Parallel.For(0, IterationCount,
                i => _redisPool.GetDb(RedisDatabase.App).SetAddAsync("Test:TestRedis:001", $"VALUE({i})"));
        }

        [Benchmark]
        public async Task TestNormal2Async()
        {
            Parallel.For(0, IterationCount,
                i => ServiceUtility.ServiceProvider.GetRequiredService<IRedisCacheConnectionPoolManager>()
                    .GetDb(RedisDatabase.App).SetAddAsync("Test:TestRedis:002", $"VALUE({i})"));
        }

        [Benchmark]
        public async Task TestCache1Async()
        {
            var appRedis = _redisPool.GetDb(RedisDatabase.App);
            Parallel.For(0, IterationCount, i => appRedis.SetAddAsync("Test:TestRedis:003", $"VALUE({i})"));
        }
        
        [Benchmark]
        public async Task TestCache2Async()
        {
            var appRedisArray = new[]
            {
                _redisPool.GetDb(RedisDatabase.App),
                _redisPool.GetDb(RedisDatabase.App),
            };
            Parallel.For(0, IterationCount,
                i => appRedisArray[i % appRedisArray.Length].SetAddAsync("Test:TestRedis:004", $"VALUE({i})"));
        }
        
        [Benchmark]
        public async Task TestCache3Async()
        {
            var appRedisArray = new[]
            {
                _redisPool.GetDb(RedisDatabase.App),
                _redisPool.GetDb(RedisDatabase.App),
                _redisPool.GetDb(RedisDatabase.App),
                _redisPool.GetDb(RedisDatabase.App),
            };
            Parallel.For(0, IterationCount,
                i => appRedisArray[i % appRedisArray.Length].SetAddAsync("Test:TestRedis:005", $"VALUE({i})"));
        }
    }
}