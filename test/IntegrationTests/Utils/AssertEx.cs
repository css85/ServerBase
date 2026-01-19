using System;
using System.Collections.Generic;
using System.Linq;
using SampleGame.Shared.Utility;
using Integration.Tests.Base;
using Integration.Tests.Client;
using Shared;
using Shared.Clock;
using Shared.Packet.Models;
using Shared.PacketModel;
using Shared.ServerModel;
using Shared.ServerModels.Common;
using Xunit;

namespace Integration.Tests.Utils
{
    public static class AssertEx
    {
        public static void NotEqualResult(int expected, int actual)
        {
            Assert.NotEqual(expected, actual);
        }
        public static void EqualResult(int expected, int actual)
        {
            Assert.Equal((ResultCode)expected, (ResultCode)actual);
        }
        public static void EqualResult(ResultCode expected, int actual)
        {
            Assert.Equal(expected, (ResultCode)actual);
        }

        public static void EqualCount<T>(int expected, IEnumerable<T> collection)
        {
            if (expected == 0)
            {
                Assert.Empty(collection);
            }
            else if (expected == 1)
            {
                Assert.Single(collection);
            }
            else
            {
                Assert.Equal(expected, collection.Count());
            }
        }
        public static void InRangeDt(long utcTicks, DateTimeOffset targetTime, TimeSpan range)
        {
            var min = targetTime - range;
            var max = targetTime + range;

            var dt = new DateTimeOffset(utcTicks, TimeSpan.Zero);
            var minDt = new DateTimeOffset(min.UtcTicks, TimeSpan.Zero);
            var maxDt = new DateTimeOffset(max.UtcTicks, TimeSpan.Zero);
            Assert.InRange(dt, minDt, maxDt);
        }
        public static void InRangeDt(long utcTicks, TimeSpan range)
        {
            var now = AppClock.OffsetUtcNow;
            var min = now - range;
            var max = now + range;

            var dt = new DateTimeOffset(utcTicks, TimeSpan.Zero);
            var minDt = new DateTimeOffset(min.UtcTicks, TimeSpan.Zero);
            var maxDt = new DateTimeOffset(max.UtcTicks, TimeSpan.Zero);
            Assert.InRange(dt, minDt, maxDt);
        }

        public static void InRangeDt(DateTime dateTime, DateTime targetTime, TimeSpan range)
        {
            var min = targetTime - range;
            var max = targetTime + range;

            Assert.InRange(dateTime, min, max);
        }
        public static void InRangeDt(DateTime dateTime, TimeSpan range)
        {
            var now = AppClock.UtcNow;
            var min = now - range;
            var max = now + range;

            Assert.InRange(dateTime, min, max);
        }

        public static void InRangeDt(DateTimeOffset dateTimeOffset, DateTimeOffset targetTime, TimeSpan range)
        {
            var min = targetTime - range;
            var max = targetTime + range;

            Assert.InRange(dateTimeOffset, min, max);
        }
        public static void InRangeDt(DateTimeOffset dateTimeOffset, TimeSpan range)
        {
            var now = AppClock.OffsetUtcNow;
            var min = now - range;
            var max = now + range;

            Assert.InRange(dateTimeOffset, min, max);
        }

        public static void EqualUserSimple(TestUserContext uc, UserSimple userSimple)
        {
            Assert.NotNull(userSimple);
            Assert.Equal(uc.UserSeq, userSimple.UserSeq);
            
        }


        public static void EmptyLanguageText(string text)
        {
            Assert.NotNull(text);
            Assert.Empty(text);
        }

    }
}
