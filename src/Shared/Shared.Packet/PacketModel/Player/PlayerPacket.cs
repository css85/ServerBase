using System;
using System.Collections.Generic;
using System.Net;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// 로그인 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Player, (byte)PLAYER_MINOR.SignIn, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/player/signin")]
    public class SignInReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>    
    [Serializable]
    public class SignInRes : ResponseBase
    {
        public long ServerTimeTick { get; set; }
        public int LoginCount { get; set; }
        public UserInfo UserInfo { get; set; } = new UserInfo();                                      // 유저 기본 
        public AttendanceInfo AttendanceInfo { get; set; } = new AttendanceInfo();                    // 출석부 정보
        public OfflineRewardInfo OfflineRewardInfo { get; set; } = new OfflineRewardInfo();           // 오프라인 보상 정보        
        public List<ItemInfo> ItemInfos { get; set; } = new List<ItemInfo>();                         // 재료 및 아이템 정보
        public List<ItemInfo> PointInfos { get; set; } = new List<ItemInfo>();                        // 포인트 정보         
        public List<CurrencyInfo> CurrencyInfos { get; set; } = new List<CurrencyInfo>();             // 재화 정보        
        public List<AccountType> AccountLinks { get; set; } = new List<AccountType>();                // 연동된 타입들        
        public VipBase VipBase { get; set; } = new VipBase();                                         // VIP 정보        
    }

    /// <summary>
    /// 닉네임 생성
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Player, (byte)PLAYER_MINOR.CreateNick, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/player/create-nick")]
    public class CreateNickReq : RequestBase
    {
        public string Nick { get; set; }
        public bool PushAgree { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-101"> InvalidParameter 잘못된 정보 넘어 왔을 경우  </response>    
    
    [Serializable]
    public class CreateNickRes : ResponseBase
    {
        public string Nick { get; set; }
    }

   
    /// <summary>
    /// 다른 유저 정보 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Player, (byte)PLAYER_MINOR.GetTargetUserData, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/player/target-data")]
    public class GetTargetDataReq : RequestBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class GetTargetDataRes : ResponseBase
    {
        public UserInfoDetail UserInfoDetail { get; set; } = new UserInfoDetail();        
        public int PostDailyCommentCount { get; set; }
        public FriendState FriendState { get; set; } = FriendState.None;
        public long RepresentPostingCoolTimeDtTick { get; set; }
    }

    
}
