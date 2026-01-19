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

    public class NewPostingInfoReq : RequestBase
    {
    
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class NewPostingInfoRes : ResponseBase
    {
        public byte Type { get; set; }
    }

}
