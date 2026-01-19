using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Server.Define;
using RedisDatabase = Shared.Server.Define.RedisDatabase;


namespace StackExchange.Redis
{
    public static class RedisExtensions
    {
        public static IDatabaseAsync GetDb(this IRedisCacheConnectionPoolManager redisPool, RedisDatabase db = RedisDatabase.None)
        {
            return redisPool.GetConnection().GetDatabase((int) db);
        }

        public static IRedisDatabase GetDb(this IRedisCacheClient source, RedisDatabase db=RedisDatabase.None,string prefix=null)
        {
            return source.GetDb((int) db, prefix);
        }

        public static Task SubscribeAsync(this ISubscriber subscriber,RedisChannel channel, Func<string, Task> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return subscriber.SubscribeAsync(channel, async (redisChannel, value) => await handler(value).ConfigureAwait(false), flags);
        }

       

    
    }
}
