using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.ServerModel;

namespace Shared.Packet.Server.Extensions
{
    public static class EntityModelExtensions
    {

        public static ItemInfo ToRewardItemInfo(this UserMailModel userMail)
        {
            return new ItemInfo(userMail.ObtainType, userMail.ObtainId, userMail.ObtainQty);
        }

        public static ItemInfo ToItemInfo(this UserGameItemOwnModel info )
        {
            return new ItemInfo(info.ObtainType, info.ItemId, info.ItemQty);
        }

        public static UserMail ToUserMail(this UserMailModel userMail)
        {
            return new UserMail
            {
                MailSeq = userMail.MailSeq,
                TitleKey = userMail.TitleKey,
                TitleKeyArgs = string.IsNullOrEmpty(userMail.TitleKeyArg) ? new string[]{""} :  userMail.TitleKeyArg.Split(','),
                LimitDtTick = userMail.LimitDt.Ticks,
                IsInfinity = userMail.LimitDt >= AppClock.MaxValue,
                rewardInfo = new ItemInfo(userMail.ObtainType, userMail.ObtainId, userMail.ObtainQty)
            };
        }

        public static void SetReceiveMail(this UserMailModel userMail)
        {
            userMail.IsObtain = true;
            userMail.ReceiveDt = AppClock.UtcNow;
        }

        public static ServerInspectionInfo ToInspetionInfo( this GateServerMaintenanceModel info )
        {
            return new ServerInspectionInfo
            {
                IsInspection = info.IsServerInspection,
                FromDt = info.InspectionFrom,
                ToDt = info.InspectionTo,   
                AllowIp = info.AllowIpInspection,
            };
        }

        public static NewUserConfig ToNewUserConfig( this NewUserConfigModel info )
        {
            return new NewUserConfig
            {
                PrologueConfigType = info.PrologueConfigType,
                Navi2Tutorial = info.Navi2Tutorial,
            };
        }
       
        //public static DateTime PossibleAdTime(this UserStoreAdInfoModel info, int coolTimeMin)
        //{
        //    return info.LastAdTime.AddMinutes(coolTimeMin);
                
        //}


        public static MissionBase ToMissionBase(this UserNavigationMissionModel info)
        {
            return new MissionBase
            {
                MissionIndex = info.MissionIndex,
                CurrentCount = info.MissionCount,
            };
        }

        public static AchievementInfo ToAchiementInfo(this UserAchievementModel info)
        {
            return new AchievementInfo
            {
                MissionIndex = info.MissionIndex,
                CurrentCount = info.MissionCount,
                LastRewardOrderNum = info.LastRewardOrderNum,
            };
        }

        public static AttendanceInfo ToAttendanceInfo(this UserAttendanceModel info)
        {
            return new AttendanceInfo
            {
                Index = info.AttendanceIndex,
                AttendanceType = info.AttendanceType,
                LastRewardDay = info.LastRewardDay,
                AttendanceDtTick = info.AttendanceDt.Ticks,
//                IsReward = info.AttendanceDt.Date != AppClock.UtcNow.Date,
                AttendNotifyLocalHour = AppClock.AttendNotifyLocalHour,
            };
        }

        public static UserAttendanceHistoryModel ToAttendanceHistory(this UserAttendanceModel info)
        {
            return new UserAttendanceHistoryModel
            {
                UserSeq = info.UserSeq,
                AttendanceIndex = info.AttendanceIndex,
                AttendanceType = info.AttendanceType,
                LastRewardDay = info.LastRewardDay,
                AttendanceDt = info.AttendanceDt,
                CreateDt = info.RegDt
            };
        }

        public static PushOptionInfo ToPushOptionInfo(this AccountModel info)
        {
            return new PushOptionInfo
            {
                ServerPush = info.ServerPush,
                NightPush = info.NightPush,
            };
        }

        public static ItemInfo ToItemInfo(this UserPointModel info)
        {
            return new ItemInfo(info.ObtainType, info.ItemId, info.ItemQty);
        }

        public static VipBase ToVipBase(this UserVipModel info)
        {
            return new VipBase
            {
                Level = info.Level,
                Point = info.Point,
                VipBenefitEndDtTick = info.ToDt.Ticks,
            };
        }

    }
}