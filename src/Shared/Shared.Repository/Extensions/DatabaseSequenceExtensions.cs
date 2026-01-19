using System;

namespace Shared.Repository.Extensions
{
    public static class DatabaseSequenceExtensions
    {
        private static readonly DateTime _targetTime = new(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        // 42byte + 11byte + 10byte
        public static long MakeSeq(long uniqueValue, int shardCount)
        {
            var totalMilliSeconds = (long) DateTime.UtcNow.Subtract(_targetTime).TotalMilliseconds;
            var shardIndex = uniqueValue % shardCount;

            return (totalMilliSeconds << 21) + (uniqueValue << 10) + shardIndex;
        }

        public static long ExtractMilliSeconds(long seq)
        {
            return seq >> 21;
        }

        public static int ExtractUniqueValue(long seq)
        {
            return (int)((seq & 0x1FFC00) >> 10);
        }

        public static int ExtractShardIndex(long seq)
        {
            // 당장 샤딩 이용 안할 예정 
            //            return (int)(seq & 0x3FF);
            return 0;
        }
    }
}