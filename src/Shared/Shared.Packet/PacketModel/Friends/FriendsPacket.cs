using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// 친구 입장
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.EnterFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/enter-friends")]
    public class EnterFriendsReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class EnterFriendsRes : ResponseBase
    {
        public string UserCode { get; set; } = string.Empty;                              // 유저 추천인 코드
        public int RecommendCount { get; set; } = 0;                                      // 현재 추천받은 수 
        public bool SendCode { get; set; } = false;                                       // 코드 보내기 1회 이상 했는지 여부 ( 코드 보내기 보상 받았었는지 )        
        public long RecommendCoolTimeEndDtTick { get; set; }                              // 다른 유저 추천인 입력 후 5일 뒤 시간   
        public bool ResponseFriendRedDot { get; set; } = false;                           // 받은 초대 레드닷
        public List<FriendInfo> Friends { get; set; } = new List<FriendInfo>();           // 내 친구 정보          
        public List<FriendBase> RequestFriends { get; set; } = new List<FriendBase>();    // 보낸 친구 정보
        public List<long> RewardRecommends { get; set; } = new List<long>();                // 추천인 보상 받은 인덱스 리스트 
    }

    /// <summary>
    /// 받은 초대 버튼 클릭
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.ResponseFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/response-friends")]
    public class ResponseFriendsReq : RequestBase
    {

    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class ResponseFriendsRes : ResponseBase
    {   
        public List<FriendBase> ResponseFriends { get; set; } = new List<FriendBase>();           // 받은 초대 정보    
        public List<long> NewFriendsSeqList { get; set; } = new List<long>(); 
    }

    /// <summary>
    /// 추천 친구 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.RecommendFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/recommend-friends")]
    public class RecommendFriendsReq : RequestBase
    {

    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RecommendFriendsRes : ResponseBase
    {
        public List<FriendBase> RecommendFriends { get; set; } = new List<FriendBase>();  
    }


    /// <summary>
    /// 친구 요청 하기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.RequestFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/request-friends")]
    public class RequestFriendsReq : RequestBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RequestFriendsRes : ResponseBase
    {
        public bool IsAcceptFriends { get; set; } = false;
        public FriendBase RequestFriend { get; set; } = new FriendBase();
        public FriendInfo FriendInfo { get; set; } = new FriendInfo();    
    }

    /// <summary>
    /// 친구 요청 취소 하기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.CancelRequestFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/cancel-request-friends")]
    public class CancelRequestFriendsReq : RequestBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class CancelRequestFriendsRes : ResponseBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <summary>
    /// 친구 요청 수락 하기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.AcceptFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/accept-friends")]
    public class AcceptFriendsReq : RequestBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class AcceptFriendsRes : ResponseBase
    {
        public FriendInfo FriendInfo { get; set; } = new FriendInfo();
    }

    /// <summary>
    /// 친구 요청 거부 하기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.RejectFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/reject-friends")]
    public class RejectFriendsReq : RequestBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RejectFriendsRes : ResponseBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <summary>
    /// 친구 삭제 하기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.DeleteFriends, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/delete-friends")]
    public class DeleteFriendsReq : RequestBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class DeleteFriendsRes : ResponseBase
    {
        public long TargetUserSeq { get; set; }
    }

    /// <summary>
    /// 크리스탈 보내기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.SendCrystal, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/send-crystal")]
    public class SendCrystalReq : RequestBase
    {
        public List<long> TargetUsers { get; set; } = new List<long>();
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class SendCrystalRes : ResponseBase
    {
        public List<FriendCoolTime> CoolTimeUsers { get; set; } = new List<FriendCoolTime>();
    }

    /// <summary>
    /// 친구 찾기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.SearchFrineds, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/search-friends")]
    public class SearchFrinedsReq : RequestBase
    {
        public string SearchNick { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class SearchFrinedsRes : ResponseBase
    {
        public List<FriendBase> SearchUsers { get; set; } = new List<FriendBase>();
    }

    /// <summary>
    /// 추천인 수 보상받기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.RewardRecommend, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/reward-recommend")]
    public class RewardRecommendReq : RequestBase
    {
        public long RewardIndex { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RewardRecommendRes : ResponseBase
    {
        public long RewardIndex { get; set; }

        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

    /// <summary>
    /// 코드 보내기 보상
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.SendCodeReward, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/sendcode-reward")]
    public class SendCodeRewardReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class SendCodeRewardRes : ResponseBase
    {
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

    /// <summary>
    /// 추천인 입력
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.RecommendUserCode, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/recommend-code")]
    public class RecommendUserCodeReq : RequestBase
    {
        public string UserCode { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RecommendUserCodeRes : ResponseBase
    {
        public long RecommendCoolTimeEndDtTick { get; set; }                              // 다른 유저 추천인 입력 후 5일 뒤 시간
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }


    /// <summary>
    /// 보낸 초대 탭 클릭
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.RequestFriendsTab, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/request-friends-tab")]
    public class RequestFriendsTabReq : RequestBase
    {

    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RequestFriendsTabRes : ResponseBase
    {
        public List<FriendBase> RequestFriends { get; set; } = new List<FriendBase>();           // 친구요청 정보    
    }

    /// <summary>
    /// 친구 탭 클릭
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Friends, (byte)FRIENDS_MINOR.FriendsTab, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/friends/friends-tab")]
    public class FriendsTabReq : RequestBase
    {

    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class FriendsTabRes : ResponseBase
    {
        public List<FriendInfo> Friends { get; set; } = new List<FriendInfo>();           // 내 친구 정보          
    }

}
