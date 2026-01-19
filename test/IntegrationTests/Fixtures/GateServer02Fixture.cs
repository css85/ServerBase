using Integration.Tests.Services;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;

namespace Integration.Tests.Fixtures
{
    public class GateServer02Fixture : GateServerFixtureBase
    {
        public GateServer02Fixture(
            ILogger<GateServer02Fixture> logger,
            ITestOutputHelperAccessor testOutputHelper,
            TestServerSessionService testServerSessionService
            ) : base(logger, testOutputHelper, testServerSessionService, 1002)
        {
        }
    }
}
