namespace Shared.Packet
{
    public enum MAJOR : byte
    {
        [PacketMajor(null)] None,

        [PacketMajor(typeof(ACCOUNT_MINOR))] Account = 1,

        [PacketMajor(typeof(PLAYER_MINOR))] Player = 2,             
        [PacketMajor(typeof(MAINSCENE_MINOR))] MainScene = 3,
        [PacketMajor(typeof(SHOPPINGMALL_MINOR))] ShoppingMall = 4,
        [PacketMajor(typeof(FRIENDS_MINOR))] Friends = 5,
        [PacketMajor(typeof(MISSION_MINOR))] Mission = 7,        
        [PacketMajor(typeof(STORE_MINOR))] Store = 8,

        [PacketMajor(typeof(GATEWAY_MINOR))] Gateway = 98,
        [PacketMajor(typeof(FRONTEND_MINOR))] Frontend = 99,
        [PacketMajor(typeof(COMMON_MINOR))] Common = 100,
        [PacketMajor(typeof(INTERNAL_MINOR))] Internal = 200,
        [PacketMajor(typeof(ADMIN_MINOR))] Admin = 201,

        [PacketMajor(typeof(TEST_MINOR))] Test = 255,
    };

    
    [PacketMinor(MAJOR.Account)]
    public enum ACCOUNT_MINOR : byte
    {
        None,
        
        Auth =1, //토큰발급 (계정생성)                
        AccountGuestToPlatform = 2,        // 플랫폼 계정 연동
        RemoveAccount = 3,                  // 계정 삭제
    };

    

    [PacketMinor(MAJOR.Frontend)]
    public enum FRONTEND_MINOR : byte
    { 
        None,
        ConnectSession,                     //인게임 시작전 세션 정보저장?

        NTFChangedApUpdateFrequencySec,     //설정변경시 알림

        NTFMessageNotification,         // 알림 메시지

        NTFAppVersionChanged,         // 앱 버전 변경됨
        NTFBundleVersionChanged,      // 번들 버전 변경됨

        GetUserLocations, // 유저 세션정보 가져오기
    }

    [PacketMinor(MAJOR.Gateway)]
    public enum GATEWAY_MINOR : byte
    {
        None,      
        GetServers,
    }

    [PacketMinor(MAJOR.Player)]
    public enum PLAYER_MINOR : byte
    {
        None,
        SignIn = 1,                 // 로그인
        CreateNick = 2,             // 닉네임 생성        
        GetTargetUserData = 4,      // 상대방 유저 정보
        PostingDetail = 15,         // 게시물 상세보기
    }

    [PacketMinor(MAJOR.MainScene)]
    public enum MAINSCENE_MINOR : byte
    {
        None,
        MainSceneEnter,
        UseCoupon,
        AttendanceEnter,
        OpenRandomBox,
        OpenFixBox
    }

    [PacketMinor(MAJOR.ShoppingMall)]
    public enum SHOPPINGMALL_MINOR : byte
    {
        None,

     

    }

    [PacketMinor(MAJOR.Friends)]
    public enum FRIENDS_MINOR : byte
    {
        None = 0,
        EnterFriends =1,            // 친구 입장
        ResponseFriends = 2,        // 받은 초대 버튼 클릭 
        RecommendFriends = 3,       // 친구 찾기 버튼 클릭 시 추천 친구 10명
        RequestFriends = 4,         // 친구 요청 하기
        CancelRequestFriends = 5,   // 친구 요청 취소
        AcceptFriends = 6,          // 친구 수락
        RejectFriends = 7,          // 친구 거부
        DeleteFriends = 8,          // 친구 삭제 
        SendCrystal = 9,            // 크리스탈 보내기
        SearchFrineds = 10,          // 친구 찾기
        RewardRecommend = 11,       // 추천인 보상
        SendCodeReward = 12,           // 코드 보내기 ( 한번도 안보냈을 경우 만 보상 지급 )
        RecommendUserCode = 13,     // 추천인 입력
        RequestFriendsTab = 14,     // 친구요청 탭
        FriendsTab = 15,            // 친구 탭

    }

    [PacketMinor(MAJOR.Mission)]
    public enum MISSION_MINOR : byte
    {
        None,
        EnterDailyMission = 1,
        EnterMission = 2,
        GetNaviMissionReward = 3,
        GetDailyMissionReward = 4,
        GetAchievementReward = 5,
        
    }

    [PacketMinor(MAJOR.Store)]
    public enum STORE_MINOR : byte
    {
        None,
        EnterStore,                     // 상점 입장
        BuyGacha,                       // 가챠 구매    

        BuyPackage,                     // 패키지 구매
        BuyInAppCheck,                  // 인앱 결제 전 체크 
        BuyInApp,                       // 인앱 결제 영수증 검증
    }


    [PacketMinor(MAJOR.Test)]
    public enum TEST_MINOR : byte
    {
        None,
        EditorDeleteAllInventory,
        ObtainItem,
        UseCurrencyMoney,        
        test
    }



    [PacketMinor(MAJOR.Common)]
    public enum COMMON_MINOR : byte
    {
        None,

        NTFPingCheck,
        Ping,

        NTFKick,
        NTFKickAll,
    }
    // 서버 내부용
    [PacketMinor(MAJOR.Internal)]
    public enum INTERNAL_MINOR : byte
    {
        None,

        NTFConnectRequest,
        NTFConnectAccept,
        NTFForward,
        NTFPing,

        NTFUpdateServer,
        NTFUpdateChannel,
        NTFUpdateChatServer,
        NTFRefreshServer,

        NTFAppVersionChanged,
        NTFBundleVersionChanged,
        NTFOpenStateChanged,

        NTFChangeCsvData,

        NTFScheduleWorkCompleted,

        NTFUpdatedGateEncryptKey,
        GetGateWayEncryptKey,
    }

    [PacketMinor(MAJOR.Admin)]
    public enum ADMIN_MINOR : byte
    {
        None,

        Command,

    };
}
