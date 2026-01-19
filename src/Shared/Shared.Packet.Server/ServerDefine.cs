using Shared.Clock;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net.Sockets;

namespace Shared.Server.Define
{
    public enum AppConfigType
    {
        None = 0,

        BundleVersion = 1,
    }

    public enum RedisDatabase
    {
        None,
        Ranking = 1,
        Account = 2,
        User = 3,
        Ingame = 4,
        App = 5,
        WebTool = 6,
        Internal = 7,
        Session = 8,
        Etc = 9,
    }

    public enum CsvItemType
    {
        None,

        Item_Consume_Line,
        Item_Upgrade_Material,
    }

    public enum ScheduleType : short
    {
        None = 0,
        ServerConnectionService = 15,

    }
    public enum UserAccessGrade : byte
    {
        User = 0,
        Gm = 1,
    }

    public enum ServerLocationType : byte
    {
        None = 0,

        LIVE = 1,               // 라이브 
        IOS_INSPECTION = 2,     // ios 검수 
        EXTERNAL_DEV = 3,       // 외부 테섭
        QA = 4,                 // QA 용 
        INTERNAL_DEV = 5,       // 내부 테섭
        LOCAL_CSS = 6,          // 디버깅용 조성수 자리 
    }


   


    public static class RedisKeys
    {
        public const string s_UserSeqCounter = "UserSeqCounter";

        public const string hs_ServerMap = "ServerMap";
        public const string hs_ServerInfoMap = "ServerInfoMap";


        public const string hs_UserSessionLocation = "UserSessionLocation";
        private const string hs_FrontendSession = "FrontendSession";

        private const string hs_MailSession = "MailSession";

        public const string keyspacePrefix = "__keyspace@{0}__:{1}";
        public const string set_keyeventPrefix = "__keyevent@{0}__:{1}";

        public const string UserToTokenMap = "UserToTokenMap:{0}";
        public const string TcpEncryptKey = "tcp:{0}";
        public const string GateWayKey = "GatewayKey";
        public const string UserTargetTypes = "UserTargetTypes";
        public const string hs_UserLoginOut = "UserLoginOut:{0}";
        public const string ss_WaitQueue = "WaitQueue:{0}";
        /// <summary>
        /// 
        /// </summary>
        public const string ss_ShoppingmallRank = "ShoppingmallRank";
        public const string ss_ContestRank = "ContestRank:{0}";
        public const string ss_LikeBestRank = "LikeBestRank:{0}";
        public const string ss_LikeBestCalculateRank = "LikeBestCalculateRank:{0}";
        public const string ss_EventStageRank = "EventStageRank:{0}";
        public const string ss_ShootingAvoidRank = "ShootingAvoidRank:{0}:{1}";
        public const string s_ServerInspection = "ServerInspeciton";                // 서버 점검 정보 ServerInspectionInfo 클래스 파싱
        public const string hs_TrendInfo = "TrendInfo";
        public const string s_StorePackageBuyCount = "Package:{0}:{1}:{2}";
        public const string s_StorePackageSpecialBuyCount = "PackageSpecial:{0}:{1}:{2}";
        public const string s_StoreBoardGachaProductBuyCount = "BoardGachaProduct:{0}:{1}:{2}";
        public const string s_StoreBoardGachaExchangeBuyCount = "BoardGachaExchange:{0}:{1}:{2}";

        public const string s_ContestSubject = "ContestSubject:{0}";
        public const string hs_NavigationMission = "NavigationMission:{0}";
        public const string hs_DailyMissionAdToken = "DailyMissionAdToken:{0}";
        public const string hs_DailyMissionComplete = "DailyMissionComplete:{0}";
        public const string hs_UserInfo = "UserInfo:{0}";        
        public const string hs_UserCashier = "UserCashier:{0}";
        public const string s_PostCommentCount = "PostCommentCount:{0}:{1}";
        public const string s_RepresentPostingCoolTime = "RepresentPostingCoolTime:{0}";
        public const string s_LastResponseFriends = "LastResponseFriends:{0}";
        public const string hs_BasicLeaflet = "BasicLeaflet:{0}";
        public const string s_UserConnect = "UserConnect:{0}";
        public const string hs_UserLastConnectTick = "UserLastConnectTick";
        public const string hs_UserFairyReward = "UserFairyReward:{0}";
        public const string s_TodayLoginCount = "LoginCount:{0}:{1}";
        public const string s_TodayNaviMissionCount = "TodayNaviMissionCount:{0}:{1}";
        public const string s_BuyPackageSpecialTotalCount = "BuyPackageSpecialTotalCount:{0}";
        public const string s_TodayGradeFailCount = "TodayGradeFailCount:{0}:{1}";
        //public const string s_UserPackageSpecial = "UserPackageSpecial:{0}:{1}";
        //public const string s_UserPackageSpecialpattern = "UserPackageSpecial:{0}:*";
        public const string s_UserBuyInAppCount = "UserBuyInAppCount:{0}";
        public const string s_UserAttendanceType = "UserAttendanceType:{0}";
        public const string s_EventStageCommentCount = "EventStageCommentCount:{0}:{1}";
        public const string hs_LastRelayView = "LastRelayView:{0}";
        public const string hs_LastADRelayView = "LastADRelayView:{0}";
        public const string hs_UserBank = "UserBank:{0}:{1}:";
        public const string hs_BingoTilePoint = "BingoTilePoint:{0}";
        public const string hs_BingoTileClear = "BingoTileClear:{0}";
        public const string ss_BingoTileRank = "BingoTileRank:{0}:{1}:{2}";
        public const string s_HiddenItemSearchProvide = "HiddenItemSearchProvide:{0}";    // 30번 이하 문제 제공 문제들
        public const string s_HiddenItemSearchAll = "HiddenItemSearchAll:{0}";            // 전체 문제들
        public const string ss_HiddenItemSearchRank = "HiddenItemSearchRank:{0}:{1}";
        public const string s_TodayMakeHiddenItemSearchCount = "TodayMakeHiddenItemSearchCount:{0}:{1}:{2}";  // 금일 문제 만든 카운트
        public const string hs_HiddenItemSearchQuestionUserBestRank = "HiddenQuestionUserBestRank:{0}:{1}";         // 내 문제 최고 순위를 모아둔 해시
        public const string s_LastPostingShareDtTick = "LastPostingShareDtTick:{0}";
        public const string s_VipStoreItemShopBuyCount = "VipStoreItemShopBuyCount:{0}:{1}:{2}";
        public const string hs_UserVip = "UserVip:{0}";
        public const string hs_CrewUserDonationWeek = "CrewUserDonationWeek:{0}:{1}";
        public const string set_CrewUser = "CrewUser:{0}";
        public const string s_UserWithdrawCrew = "UserWithdrawCrew:{0}";
        public const string s_UserCrewDonationCount = "UserCrewDonationCount:{0}:{1}";
        public const string s_CrewShopBuyCount = "CrewShopBuyCount:{0}:{1}:{2}";        
        public const string s_CrewGiftBuyCount = "CrewGiftBuyCount:{0}:{1}:{2}";
        public const string hs_CrewBuffInfo = "hs_CreBuffInfo:{0}";


        public static string ServiceSessionKey(NetServiceType serviceType) => serviceType switch
        {
            NetServiceType.FrontEnd => hs_FrontendSession,         
            _ => throw new ArgumentException()
        };
        public static string GetPostCommentCountKey(long userSeq) => string.Format(s_PostCommentCount, AppClock.UtcNow.ToShortDateString(), userSeq);
        public static string GetEventStageCommentCountKey(long userSeq) => string.Format(s_EventStageCommentCount, AppClock.UtcNow.ToShortDateString(), userSeq);
        public static string GetTodayLoginCountKey(long userSeq) => string.Format(s_TodayLoginCount, AppClock.UtcNow.ToShortDateString(), userSeq);
        public static string GetTodayNaviMissionCountKey(long userSeq) => string.Format(s_TodayNaviMissionCount, AppClock.UtcNow.ToShortDateString(), userSeq);
        public static string GetTodayGradeFailCountKey(long userSeq) => string.Format(s_TodayGradeFailCount, AppClock.UtcNow.ToShortDateString(), userSeq);
        public static string GetBingoTileRankKey(long bingoIndex, int tileType, long tileIndex) => string.Format(ss_BingoTileRank, bingoIndex, tileType, tileIndex);
        public static string GetHiddenItemSearchRankKey(long eventstageIndex, string dateCycleKey) => string.Format(ss_HiddenItemSearchRank, eventstageIndex, dateCycleKey);
        public static string GetHiddenItemSearchQuestionUserBestRankKey(long eventstageIndex, string dateCycleKey) => string.Format(hs_HiddenItemSearchQuestionUserBestRank, eventstageIndex, dateCycleKey);
        public static string GetTodayMakeHiddenItemSearchCountKey(long eventstageIndex, long userSeq) => string.Format(s_TodayMakeHiddenItemSearchCount, eventstageIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
        public static string GetCrewUserDonationWeekKey(long crewSeq) => string.Format(hs_CrewUserDonationWeek, AppClock.GetWeekString(AppClock.UtcNow), crewSeq);


        public static string GetPackageKey(CycleType cycle, long packageIndex, long userSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_StorePackageBuyCount, packageIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_StorePackageBuyCount, packageIndex, AppClock.GetWeekString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_StorePackageBuyCount, packageIndex, AppClock.GetMonthString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_StorePackageBuyCount, packageIndex, "Nocycle", userSeq);

            return null;
        }
        public static string GetPackageSpecialKey(CycleType cycle, long packageIndex, long userSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_StorePackageSpecialBuyCount, packageIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_StorePackageSpecialBuyCount, packageIndex, AppClock.GetWeekString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_StorePackageSpecialBuyCount, packageIndex, AppClock.GetMonthString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_StorePackageSpecialBuyCount, packageIndex, "Nocycle", userSeq);

            return null;
        }

        public static string GetBoardGachaProductKey(CycleType cycle, long packageIndex, long userSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_StoreBoardGachaProductBuyCount, packageIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_StoreBoardGachaProductBuyCount, packageIndex, AppClock.GetWeekString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_StoreBoardGachaProductBuyCount, packageIndex, AppClock.GetMonthString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_StoreBoardGachaProductBuyCount, packageIndex, "Nocycle", userSeq);

            return null;
        }
        public static string GetBoardGachaExchangeKey(CycleType cycle, long packageIndex, long userSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_StoreBoardGachaExchangeBuyCount, packageIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_StoreBoardGachaExchangeBuyCount, packageIndex, AppClock.GetWeekString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_StoreBoardGachaExchangeBuyCount, packageIndex, AppClock.GetMonthString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_StoreBoardGachaExchangeBuyCount, packageIndex, "Nocycle", userSeq);

            return null;
        }

        public static string GetVipStoreItemShopProductKey(CycleType cycle, long productIndex, long userSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_VipStoreItemShopBuyCount, productIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_VipStoreItemShopBuyCount, productIndex, AppClock.GetWeekString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_VipStoreItemShopBuyCount, productIndex, AppClock.GetMonthString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_VipStoreItemShopBuyCount, productIndex, "Nocycle", userSeq);

            return null;
        }

        public static string GetCrewShopProductKey(CycleType cycle, long productIndex, long userSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_CrewShopBuyCount, productIndex, AppClock.UtcNow.ToShortDateString(), userSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_CrewShopBuyCount, productIndex, AppClock.GetWeekString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_CrewShopBuyCount, productIndex, AppClock.GetMonthString(AppClock.UtcNow), userSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_CrewShopBuyCount, productIndex, "Nocycle", userSeq);

            return null;
        }
        
        public static string GetCrewGiftProductKey(CycleType cycle, long productIndex, long crewSeq)
        {
            if (cycle == CycleType.Daily)
                return string.Format(s_CrewGiftBuyCount, productIndex, AppClock.UtcNow.ToShortDateString(), crewSeq);
            else if (cycle == CycleType.Weekly)
                return string.Format(s_CrewGiftBuyCount, productIndex, AppClock.GetWeekString(AppClock.UtcNow), crewSeq);
            else if (cycle == CycleType.Monthly)
                return string.Format(s_CrewGiftBuyCount, productIndex, AppClock.GetMonthString(AppClock.UtcNow), crewSeq);
            else if (cycle == CycleType.Account)
                return string.Format(s_CrewGiftBuyCount, productIndex, "Nocycle", crewSeq);

            return null;
        }

    }


    public static class RedisPubSubChannels
    {
        public const string RefreshCsv = "RefreshCsv";
        public const string InspectionInfo = "InspectionInfo";
        public const string TrendInfo = "TrendInfo";
        public const string RefreshLikeBest = "RefreshLikeBest";
        public const string RefreshShoppingmallRank = "RefreshShoppingmallRank";
        public const string NewUserConfig = "NewUserConfig";

    }

    public static class RedisScripts
    {
        public const string SortedSetModify =
            @"
redis.call('ZREM', @key, @oldMember)
return redis.call('ZADD', @key, @score, @newMember)";
        public const string ListLeftPushTrimWhenOverCount =
            @"
if redis.call('LPUSH', @key, @value) > tonumber(@start)
then return redis.call('LTRIM', @key, @start, @stop) end";
    }

    public static class RedLockKeys
    {
        public static readonly TimeSpan DefaultExpiryTime = TimeSpan.FromSeconds(20);
        public static readonly TimeSpan DefaultWaitTime = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan DefaultRetryTime = TimeSpan.FromMilliseconds(300);

        
        public static string BuyIAPLock(string tid) => $"BuyIAPLock:{tid}";
        public static string BuyAdStoreLock(string adToken) => $"BuyAdStoreLock:{adToken}";
        public static string ResultStateLock(byte gameMode, long fightIndex) => $"ResultStateLock:{gameMode}:{fightIndex}";
        public static string ResultSuccessLock(byte gameMode, long fightIndex) => $"ResultSuccessLock:{gameMode}:{fightIndex}";
        public static string MatchResultLock(string matchId, long userSeq) => $"MatchResultLock:{matchId}:{userSeq}";
        public static string AttendacneRewardLock(long userSeq) => $"AttendRewardLock:{userSeq}";
        public static string UseCouponLock(long userSeq) => $"UseCouponLock:{userSeq}";
        public static string ReceiveMailLock(long userSeq) => $"ReceiveMailLock:{userSeq}";
        public static string BingoDonateLock(long userSeq) => $"BingoDonateLock:{userSeq}";
        public static string CrewCreateLock(string name) => $"CrewCreateLock:{name}";
        public static string CrewJoinLock(long crewSeq) => $"CrewJoinLock:{crewSeq}";
        public static string CrewGiftLock(long crewGiftSeq, int slot) => $"CrewGiftLock:{crewGiftSeq}:{slot}";

    }

    public class DefineConsts
    {
//        public const int MaxDurability = 15;
//        public const int DefaultEquipDurability = 15;
//        public const decimal RepairAmount = 20.0M;        

        //public const int MinLevel = 1;
        //public const int MaxLevel = 99;
//        public const double LuckyProbUpProb = 0.10d;
//        public const int LuckyProbUpEventMulti = 30;
//        public const int LuckyProbUpMin = 15;
//        public const double AutoFee = 0.3d;
//        public const int NotifyUpgrade = 5;
//        public const double NotifyCatchFishLength = 200d;
//        public const int AquariumDecLengthCycleMin = 30;
        //        public const decimal AquariumDecLengthPer = 0.005M;        
        // !!!!!!!!!!!! 변경시 주석 참고 !!!!!!!!!!!! 
//        public const decimal AquariumDecLengthPer = 0M;       // 0 에서 0보다 큰 값으로 변경 시 db의 last_calc_dt 값 전체 현재 시간으로 업데이트 필요
//        public const int AquariumDecConditionCycleMin = 30;
        // !!!!!!!!!!!! 변경시 주석 참고 !!!!!!!!!!!!
//        public const double AquariumDecCondition = 1d;        // 0 에서 0보다 큰 값으로 변경 시 db의 last_calc_condtion_dt 값 전체 현재 시간으로 업데이트 필요        
//        public const long MaxTreasureBoxPoint = 30;
//        public const int ResearchVoteLimitHour = 18;

        // 레이싱 대기 시작 시간으로 부터 걸리는 시간 
//        public const int RacingParticipateTime = 2;     // 2분 후 참가하기 시작 
//        public const int RacingBettingTime = 10;        // 10분 후 응원하기 시작
//        public const int RacingViewStartTime = 25;      // 25분 후 레이싱 보기 시작
//        public const int RacingViewEndTime = 30;        // 30분 후 종료 

        //public const int RacingParticipateTime = 1;     // 1분 후 참가하기 시작 
        //public const int RacingBettingTime = 3;        // 3분 후 응원하기 시작
        //public const int RacingViewStartTime = 6;      // 6분 후 레이싱 보기 시작
        //public const int RacingViewEndTime = 10;        // 10분 후 종료 


        public const string JwtAuthenticationScheme = "Bearer";

        public const string WarningMessageLogName = "warning";
        public const string ErrorMessageLogName = "error";

        public static readonly string[] MessageLogNames = {
            WarningMessageLogName,
            ErrorMessageLogName,
        };

    }
}
