using System;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel.Test
{
    /// <summary>
    /// (테스트) 아이템 획득
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Test, (byte)TEST_MINOR.ObtainItem, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, "/api/v2/test/store/obtain-item")]
    public class TestObtainItemReq : RequestBase
    {
        public RewardPaymentType ObtainType;
        public long ItemId;
        public string UniqueKey;
        public int ItemQty;
        public int Period;
    }

    [Serializable]
    public class TestObtainItemRes : ResponseBase
    {
        public ItemInfo ItemInfo;
    }
}
