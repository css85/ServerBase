using System.Collections.Generic;
using System.Linq;
using Integration.Tests.Base;
using Shared;
using Shared.Model;
using Shared.Models;
using Shared.Packet.Models;
using Shared.PacketModel;

namespace Integration.Tests.Client
{
    public partial class TestUserContext
    {
        public long UserSeq { get; private set; }
        public string LoginId { get; private set; }
        public string Token { get; private set; }
        public string GateWayKey { get; private set; }        

        private void OnReceive(IPacketData data, int result)
        {
            if (result != 0)
                return;

            if (data is ConnectSessionRes)
            {
                var testClient = _testClientMap[NetServiceType.FrontEnd];
                if (testClient is TestSessionClient)
                {
                    var response = (ConnectSessionRes)data;
                    testClient.SetEncryptKey(response.EncryptKey);
                }
            }
            
        }
    }
}
