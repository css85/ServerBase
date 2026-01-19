using Integration.Tests.Services;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;

namespace Integration.Tests.Fixtures
{
    public class FrontendServer02Fixture : FrontendServerFixtureBase
    {
        public FrontendServer02Fixture(
            ILogger<FrontendServer02Fixture> logger,
            ITestOutputHelperAccessor testOutputHelper,
            TestServerSessionService testServerSessionService
        ) : base(logger, testOutputHelper, testServerSessionService, 2002)
        {
        }
    }
}