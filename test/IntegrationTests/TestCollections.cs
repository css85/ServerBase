using Integration.Tests.Fixtures;
using Xunit;

namespace Integration.Tests
{
    [CollectionDefinition(nameof(DefaultTestCollections), DisableParallelization = false)]
    public class DefaultTestCollections :
        ICollectionFixture<FrontendServer01Fixture>,
        ICollectionFixture<FrontendServer02Fixture>,
        ICollectionFixture<GateServer01Fixture>,
        ICollectionFixture<GateServer02Fixture>
    {
    }

    [CollectionDefinition(nameof(SingleServerTestCollections),DisableParallelization = true)]
    public class SingleServerTestCollections :
        ICollectionFixture<FrontendServer01Fixture>,
        ICollectionFixture<GateServer01Fixture>
    {
    }
}
