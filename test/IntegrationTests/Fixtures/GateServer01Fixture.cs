using Integration.Tests.Services;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;

namespace Integration.Tests.Fixtures
{
    public class GateServer01Fixture : GateServerFixtureBase
    {
        public GateServer01Fixture(
            ILogger<GateServer01Fixture> logger,
            ITestOutputHelperAccessor testOutputHelper,
            TestServerSessionService testServerSessionService
            ) : base(logger, testOutputHelper, testServerSessionService, 1001)
        {
        }
    }
}
