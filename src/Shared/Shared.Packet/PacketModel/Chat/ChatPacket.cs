using System;
using System.Collections.Generic;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// 월드 채팅
    /// </summary>
    [Serializable]
    //[RequestClass((byte)MAJOR.Contest, (byte)CONTEST_MINOR.EnterContest, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/contest/contest-enter")]

    public class WorldChatMessageReq : RequestBase
    {
        public string Message { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class WorldChatMessageRes : ResponseBase
    {
        public long Rank { get; set; }
        public UserInfo UserInfo { get; set; }
        public string Message { get; set; }
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }


    /// <summary>
    /// 포스팅 자랑하기
    /// </summary>
    [Serializable]    
    public class WorldChatPostingReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class WorldChatPostingRes : ResponseBase
    {
        public long Rank { get; set; }
        public UserInfo UserInfo { get; set; }

        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

}
