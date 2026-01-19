using Integration.Tests.Services;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;

namespace Integration.Tests.Fixtures
{
    public class FrontendServer01Fixture : FrontendServerFixtureBase
    {
        public FrontendServer01Fixture(
            ILogger<FrontendServer01Fixture> logger,
            ITestOutputHelperAccessor testOutputHelper,
            TestServerSessionService testServerSessionService
            ) : base(logger, testOutputHelper, testServerSessionService, 2001)
        {
        }
    }
}
