using System;
using System.Threading.Tasks;
using Shared.Clock;

namespace Integration.Tests.Tests
{
    public class TestAppClockContext : IDisposable
    {
        public TestAppClockContext(TimeSpan offset)
        {
            AppClock.SetOffset(offset);
        }

        public void AddOffset(TimeSpan offset)
        {
            AppClock.AddOffset(offset);
        }

        public void Dispose()
        {
            AppClock.SetOffset(TimeSpan.Zero);
        }
    }
}