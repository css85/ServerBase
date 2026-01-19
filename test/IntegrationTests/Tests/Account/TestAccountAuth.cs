using System;
using System.Threading.Tasks;
using Integration.Tests.Fixtures;
using Integration.Tests.Utils;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.PacketModel;
using Xunit;

namespace Integration.Tests.Tests.Account
{
    [Collection(nameof(DefaultTestCollections))]
    public class TestAccountAuth : UserTestBase
    {
        private readonly ILogger<TestAccountAuth> _logger;

        public TestAccountAuth(
            ILogger<TestAccountAuth> logger,
            FrontendServer01Fixture frontendServer01Fixture,
            FrontendServer02Fixture frontendServer02Fixture,            
            GateServer01Fixture gateServer01Fixture,
            GateServer02Fixture gateServer02Fixture
        ) : base(logger, frontendServer01Fixture, frontendServer02Fixture, 
            gateServer01Fixture, gateServer02Fixture)
        {
            _logger = logger;
        }

        [Fact]
        public async Task AccountAuth_Normal_SuccessAsync()
        {
            // Arrange
            //var addProvider = Provider.Google;
            //var addId = Guid.NewGuid().ToString();
            //await using var uc1 = await MakeUcBaseAsync();

            //// Act
            //var result1 = await uc1.SendPacketAsync<GateAccountAuthRes>(new GateAccountAuthReq{
            //    Provider = addProvider,
            //    Id = addId,
            //});

            //// Assert
            //AssertEx.EqualResult(ResultCode.Success, result1.ResultCode);
        }
    }
}
