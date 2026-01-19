using System;
using Shared.Model;
using Shared.Packet;

namespace Shared.PacketModel.Test
{
    /// <summary>
    /// (테스트) 재화 사용
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Test, (byte)TEST_MINOR.UseCurrencyMoney, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, "/api/v2/test/store/use-currency-money")]
    public class TestUseCurrencyMoneyReq : RequestBase
    {
        public CurrencyType CurrencyType; // enum CurrencyMoneyType
        public int Amount;
    }

    [Serializable]
    public class TestUseCurrencyMoneyRes : ResponseBase
    {
    }
}