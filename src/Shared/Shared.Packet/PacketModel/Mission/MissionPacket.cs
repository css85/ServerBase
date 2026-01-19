using System;
using System.Collections.Generic;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// 일일미션 입장
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.Mission, (byte)MISSION_MINOR.EnterDailyMission, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mission/enter-daily")]
    public class EnterDailyMissionReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class EnterDailyMissionRes : ResponseBase
    {
        public long DailyMissionToDtTick { get; set; }
        public List<MissionInfo> MissionInfos { get; set; } = new List<MissionInfo>();
        public DailyMissionBonusInfo BonusInfo { get; set; } = new DailyMissionBonusInfo();
    }

    /// <summary>
    /// 미션 입장
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.Mission, (byte)MISSION_MINOR.EnterMission, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mission/enter-mission")]
    public class EnterMissionReq : RequestBase
    {

    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class EnterMissionRes : ResponseBase
    {
        public long DailyMissionToDtTick { get; set; }
        public List<MissionInfo> DailyInfos { get; set; } = new List<MissionInfo>();
        public DailyMissionBonusInfo BonusInfo { get; set; } = new DailyMissionBonusInfo();

        public List<AchievementInfo> AchievementInfos { get; set; } = new List<AchievementInfo>();
    }


    /// <summary>
    /// 일일 미션 보상 받기 
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.Mission, (byte)MISSION_MINOR.GetDailyMissionReward, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mission/daily-reward")]
    public class RewardDailyMissionReq : RequestBase
    {
        public long MissionIndex { get; set; }
    }


    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-104"> NotFound csv 데이터가 없을 경우   </response>        
    /// <response code="-1503"> NotMissionSuccess 미션을 완료하지 못함   </response>    
    /// <response code="-1504"> AlreadyMissionReward 이미 미션 보상 받음   </response>  
    /// 
    [Serializable]
    public class RewardDailyMissionRes : ResponseBase
    {
        public long MissionIndex { get; set; }
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();

        public List<MissionInfo> MissionInfos { get; set; } = new List<MissionInfo>();
        public DailyMissionBonusInfo BonusInfo { get; set; } = new DailyMissionBonusInfo();
    }


    /// <summary>
    /// 반복업적 보상 받기 
    /// </summary>        
    [Serializable]
    [RequestClass((byte)MAJOR.Mission, (byte)MISSION_MINOR.GetAchievementReward, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/mission/achiev-reward")]
    public class RewardAchievementMissionReq : RequestBase
    {
        public long MissionIndex { get; set; }
    }


    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-104"> NotFound csv 데이터가 없을 경우   </response>        
    /// <response code="-1503"> NotMissionSuccess 미션을 완료하지 못함   </response>    
    /// <response code="-1504"> AlreadyMissionReward 이미 미션 보상 받음   </response>  
    /// 
    [Serializable]
    public class RewardAchievementMissionRes : ResponseBase
    {
        public long MissionIndex { get; set; }
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();

        public AchievementInfo AchievementInfo { get; set; } = new AchievementInfo();
    }

}
