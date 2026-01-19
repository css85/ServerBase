using System.Threading.Tasks;
using RedLockNet;
using RedLockNet.SERedis;
using Shared.Server.Define;

namespace Shared.ServerApp.Extensions
{
    public static class RedLockExtensions
    {
        public static Task<IRedLock> CreateLockAsync(this RedLockFactory redLockFactory, string redLockKey)
        {
            return redLockFactory.CreateLockAsync(redLockKey, RedLockKeys.DefaultExpiryTime,
                RedLockKeys.DefaultWaitTime, RedLockKeys.DefaultRetryTime);
        }
    }
}