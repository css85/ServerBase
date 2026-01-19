using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;
using System;
using System.Collections.Generic;

namespace Shared.PacketModel
{
    /// <summary>
    /// 메인씬 입장 패킷
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.MainScene, (byte)MAINSCENE_MINOR.MainSceneEnter, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mainscene/mainscene-enter")]
    public class MainSceneEnterReq : RequestBase
    {        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class MainSceneEnterRes : ResponseBase
    {
        public List<MainSceneRedDotType> RedDots { get; set; } = new List<MainSceneRedDotType>();
        public bool IsNotifyInspection { get; set; }
        public InspectionPacketInfo InspectionInfo { get; set; } = new InspectionPacketInfo();
    }


    /// <summary>
    /// 쿠폰 사용
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.MainScene, (byte)MAINSCENE_MINOR.UseCoupon, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mainscene/use-coupon")]
    public class UseCouponReq : RequestBase
    {
        public string CouponCode { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class UseCouponRes : ResponseBase
    {   public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

    /// <summary>
    /// 7일 출석부 입장
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.MainScene, (byte)MAINSCENE_MINOR.AttendanceEnter, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mainscene/attendance-enter")]
    public class AttendanceEnterReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class AttendanceEnterRes : ResponseBase
    {
        public AttendanceInfo AttendanceInfo { get; set; } = new AttendanceInfo();
        public int LoginCount { get; set; }    // 240423

        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

    

    /// <summary>
    /// 랜덤박스 오픈
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.MainScene, (byte)MAINSCENE_MINOR.OpenRandomBox, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mainscene/open-randombox")]
    public class OpenRandomBoxReq : RequestBase
    {
        public long BoxIndex { get; set; }
        public int OpenBoxCount { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class OpenRandomBoxRes : ResponseBase
    {
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

    /// <summary>
    /// 확정박스 오픈
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.MainScene, (byte)MAINSCENE_MINOR.OpenFixBox, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mainscene/open-fixbox")]
    public class OpenFixBoxReq : RequestBase
    {
        public long BoxIndex { get; set; }
        public int OpenBoxCount { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class OpenFixBoxRes : ResponseBase
    {
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

}