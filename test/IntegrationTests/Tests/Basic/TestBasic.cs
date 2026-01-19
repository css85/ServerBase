using System.Threading.Tasks;
using Integration.Tests.Fixtures;
using Integration.Tests.Utils;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.PacketModel;
using Xunit;

namespace Integration.Tests.Tests.Basic
{
    [Collection(nameof(DefaultTestCollections))]
    public class TestBasic : UserTestBase
    {
        private readonly ILogger<TestBasic> _logger;

        public TestBasic(
            ILogger<TestBasic> logger,
            FrontendServer01Fixture frontendServer01Fixture,
            FrontendServer02Fixture frontendServer02Fixture,
            GateServer01Fixture gateServer01Fixture,
            GateServer02Fixture gateServer02Fixture
        ) : base(logger, frontendServer01Fixture, frontendServer02Fixture, gateServer01Fixture, gateServer02Fixture)
        {
            _logger = logger;
        }

        [Fact]
        public async Task Test_MakeUser_SuccessAsync()
        {
            //Arrange
            await using var uc = await MakeUcBaseAsync();            

            // Act
           

            var connectFrontendResult = await uc.SendPacketAsync<ConnectSessionRes>(new ConnectSessionReq
            {
                UserSeq = uc.UserSeq,
                Language = 0,
                OsType = (byte) OSType.Android,
                AppVer = "1",
            });

            
            //Assert
            AssertEx.EqualResult(ResultCode.Success, connectFrontendResult.ResultCode);
            AssertEx.EqualResult(ResultCode.Success, connectFrontendResult.ResultCode);            
        }
        [Fact]
        public async Task Test_MakeUc_SuccessAsync()
        {
            //Arrange

            // Act
            await using var uc = await MakeUcAsync();

            //Assert
            Assert.NotNull(uc);
        }
    }
}
