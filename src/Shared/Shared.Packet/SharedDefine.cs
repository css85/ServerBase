using Shared.Packet.Models;

/// <summary>
/// 
/// </summary>
namespace Shared
{

    public static class SharedDefine
    {
        
        public const int MAX_FRIENDS = 50;              // 친구 최대 수
        public const int MAX_REQUEST_FRIENDS = 20;      // 친구 요청 최대 수
        public const int MAX_RESPONSE_FRIENDS = 20;      // 친구 요청 최대 수                
        public const int ATTENDANCE_RETURN_DAY = 15;
        public const int RECOMMEND_CODE_COOLTIME_MIN = 60 * 24 * 5;   // 추천인 코드 보내기 쿨타임
    }

    public enum ProtocolType
    {
        None,

        // ex
        Tcp,
        Http,
        WebSocket, // 사용 안함
    }

// 수정시 MultipleNetServiceTypeExtensions 확인
    public enum NetServiceType
    {
        None = 0,
        Gate = 1, // 게이트웨이역활   http
        Auth = 2, // 로그인서버      http
        Api = 3, // api서버        http
        Chat = 4, // 채팅 서버       signalR
        FrontEnd = 5, // 프론트엔드서버   tcp
        
        Internal = 100, // 서버 내부 통신용 tcp
        Admin = 101, // Admin http
    }

    public enum RequestMethodType
    {
        None,
        Post,
        Get,
    }

    public enum PacketType : byte
    {
        None,

        Request,
        Response,
        Ntf,
    }

    public enum ExtHeader
    {
        None,
        Auth,
        Session,
        Custom,
        Test1,
        Test2,
        Max,
    }

    public enum ResultCode
    {
        Success = 0,
        InvalidToken = -1, // 잘못된 토큰값
        PrevToken = -2, // 예전 토큰임
        ExpiredToken = -3, // 토큰 만료됨 (7일)
        NotAuthorized = -4, // 토큰 인증 안됨
        NotConnected = -5, // 연결 안됨
        NotSupportedPacket = -6, // 지원하지 않는 패킷
        ServerError = -7, // 서버 에러
        VerifyPermissionsFailed = -8, //접근 권한인증 실패
        NotSupportedFeature = -9, //지원하지 않는 기능
        ServerInspection = -10, // 서버 점검중 
        WrongAccount = -11,     // 잘못된 계정
        BlockCountry = -12,     // 접속 할 수 없는 국가         
        NotSupportVersion = -13, // 사용할 수 없는 버전
        BlockUser = -14,        // 블럭 된 유저
        DuplicatePacket = -15,      // 중복 패킷 들어와서 이전 데이터 처리중

        InvalidParameter = -101, // 잘못된 값
        Fail = -102, // 실패
        Timeout = -103, // 실패 (유닛테스트)
        NotFound = -104, // 해당정보 찾을수없음
        InvalidLanguage = -105, // 잘못된 언어
        RedLockAcquireFailed = -106, // RedLock lock 획득 실패

        AlreadyConnectedUser = -107, //중복된 UserSeq
        Slang = -108, // 비속어                      
        ExistUserAccount = -109,     // 계정이 존재
        ExistNick = -110,           // 닉네임 존재
        NotEnoughGold = -111,       // 골드 부족                
        NotEnoughItem = -112,       // 사용 할 아이템 부족        
        NotEnoughRuby = -113,       // 루비 부족
        NotEnoughParts = -114,      // 파츠 부족
        NotEnoughRareTicket = -115,       // Rare 티켓 부족
        NotEnoughSpecialTicket = -116,       // 스페셜 티켓 부족
        NotEnoughContestCoin = -117,       // 콘테스트 코인 부족
        NotEnoughContestTicket = -118,       // 콘테스트 티켓 부족
        NotEnoughCurrency = -119,           // 재화 부족
        ReceiptValidationFailed = -120,           // 영수증 검증 실패
        ReceiptAlreadyProcessed = -121,        // 이미 처리된 영수증
        NotEnoughPoint = -122,               // 포인트 부족
        ExistPlatformLink = -123,           // 이미 연동된 플랫폼 계정 존재 
        NotEnoughCrewPoint = -124,          // 크루 포인트 부족

        // 내부 통신
        InternalFail = -201,
        InternalRecallNotSupported = -202, // 회수 지원 안되는 타입


        AlreadyMaxPartsLevel = -1001,       // 파츠 레벨업 시 이미 최대 레벨 일 경우 
        NotEnoughPartsLevel = -1002,        // 파츠 등급 업 시 레벨 부족
        MaxPartsGrade = -1003,              // 파츠 등급 업 시 이미 최대 등급
        ImpossibleMaterialCraft = -1004,    // 재료 제작 시 필요한 재료들이 테이블에서 누락 되었을떄 ( material_combine 에서 없을때 )
        ImpossiblePartsCraft = -1005,       // 파츠 제작 시 레벨 부족
        NotEnoughLevel = -1006,             // 쇼핑몰 등급 업 시 레벨 부족
        CatalogCoverCoolTime = -1007,       // 대표 카탈로그 등록 시 쿨타임 중일 경우
        MaxMyAlbumSlot = -1008,             // 마이앨범 슬롯이 이미 최대다.  
        NotEnoughUnlockCatalogGrade = -1009, // 카탈로그 언락 조건 등급 부족
        NotEnoughUnlockStockGrade = -1010,  // 상품등록 슬롯 언락 조건 등급 부족
        AlreadyCollectionReward = -1011,    // 이미 컬렉션 보상 받음
        NotChangeColorPartsType = -1012,    // 파츠 색상 못바꾸는 타입

        ExistsNickChangeTicket = -1101,     // 닉네임 티켓이 있는데 루비 사용으로 닉네임 패킷이 들어올 경우 
        MaxPosting = -1102,                 // 프로필 게시물 등록 시 이미 게시물 수가 이미 최대일 경우
        AlreadyLikePosting = -1103,         // 이미 좋아요 누른 게시물인데 또 좋아요 할 경우
        NotExistLikePosting = -1104,        // 좋아요 취소 하려는데 좋아요 한 적이 없음
        MaxDailyPostComment = -1105,        // 일일 댓글 달기 갯수 최대로 사용 
        RemovedPosting = -1106,             // 지워진 게시물 입니다. 
        RemovedComment = -1107,             // 지워진 댓글 입니다. 
        NotAuthToDelete = -1108,            // 댓글 삭제할 권한이 없습니다. 
        RepresentPostingCoolTime = -1109,   // 대표 게시물 쿨타임중
        LikeBestRankPosting = -1110,        // 현재 좋아요 베스트 랭킹에 올라가 있는 포스팅입니다. 

        NotActiveContest = -1201,           // 콘테스트 기간이 아님
        NotWearPartsContest = -1202,        // 옷을 입지 않음
        MaxPurchaseTicket = -1203,          // 콘테스트 티켓 구매 시 일일 최대로 구매 

        NotSellProduct = -1301,             // 판매하지 않는 상품 ( 가챠 상품의 해당 인덱스가 없거나 보상이 없거나 isView가 false )
        StoreLimit = -1302,                 // 구매 제한 횟수 초과 ( 가챠, 패키지 ) 
        NotSellTime = -1303,                // 판매 시간이 아님
        NotAdCoolTime = -1304,              // 광고 시간이 아님
        WrongStorePacketModel = -1305,      // 광고 상품과 일반 상품의 패킷 분류로 인하여 광고 상품구매시 일반 상품 구매 패킷으로 올 경우         
        StoreLimitAd = -1306,               // 광고 제한 횟수 초과
        ImpossibleBuyPackage = -1307,       // 구매 할 수 없는 패키지
        StoreServerTotalLimit = -1308,      // 구매 제한 횟수 초과 ( 서버 통합 ) 

        AlreadyMaxStaffIndex = -1401,       // 이미 최대치 점원을 보유 중 
        NotEnoughUnlockStaffGrade = -1402,  // 점원 구매 시 쇼핑몰 등급 부족
        AlreadyMaxStaffLevel = -1403,       // 이미 최대 레벨 점원
        ExistStaffTool = -1404,             // 이미 보유 한 홍보도구
        NotEnoughUnlockStaffLevel = -1405,  // 점원 홍보도구 구매시 쇼핑몰 레벨 부족
        NotOwnStaffTool = -1406,            // 보유하고 있지 않은 홍보도구 장착 시도 

        NotMissionTime = -1501,             // 미션 시간이 아님 ( 시간제 미션 ) 
        MissionLimitMax = -1502,            // 미션 보상 제한 갯수가 최대 
        NotMissionSuccess = -1503,          // 미션 보상 시 미션 성공을 아직 못한 상태로 보상 패킷이 왔을 경우 
        AlreadyMissionReward = -1504,       // 이미 미션 보상 받음
        NotCompleteTwodayBonus = -1505,     // 2일보너스 미달성

        MaxFriends = -1601,                 // 친구 최대 치 
        MaxRequestFriends = -1602,          // 친구 요청 보낸 초대 20명 최대 치 
        MaxResponseFriends = -1603,          // 친구 요청 받은 수  20명 최대 치 
        AlreadyFriends = -1604,             // 이미 친구인 상태
        NotExistReqFriends = -1605,         // 요청한 친구 목록에 없음
        NotExistResFriends = -1606,         // 요청받은 친구 목록에 없음
        NotFriends = -1607,                 // 친구가 아님
        AlreadyRewardRecommend = -1608,     // 이미 추천 보상 받음
        AlreadyRequestFriends = -1609,      // 이미 보낸 친구
        NotExistFriendCode = -1610,         // 친구 코드 잘못 됨

        WrongCouponCode = -2101,            // 잘못 된 쿠폰코드
        UsedCouponCode = -2102,             // 사용 된 쿠폰코드
        NotCouponTime = -2103,              // 쿠폰 사용 시간이 아님
        LimitCouponCount = -2104,           // 쿠폰 사용 제한 갯수 초과
        AlreadyAdOfflineReward = -2105,     // 이미 광고 오프라인 보상 받음

    }



    public enum CurrencyType
    {
        None = 0,

        Free = 1,
        Gold = 2,
        Ruby = 3,
        Heart = 8,
        NicknameTicket = 10, 
        Crystal = 11,
        GachaMileage = 13,
        ADRemovalTicket = 19,
        VipMileage = 25,
    }

  

    public enum PointType : byte
    {
        None,
        ShoppingmallManage = 1,
        VipPoint = 3,               // VIP 포인트는 Point 테이블에 저장하지 않으며 각 유저별로 레벨과 함께 별도 관리
    }

    public enum EventStageItemType : byte
    {
        None,
        HiddenItemSearch_Hourglass,        // 돋보기
        HiddenItemSearch_Magnifier         // 모래시계

    }

    public enum RewardPaymentType : byte
    {
        None,
        Currency = 1,               // 재화
        Material = 2,               // 재료  
        Point = 3,                  // 포인트
        Iap = 4,                    // 인앱        
        Box = 5,                    // 박스류 
    }


    public enum CycleType : byte
    {
        None, 
        
        Account,
        Daily,
        Weekly,
        Monthly,

    }

    public enum NotificationType : byte
    {
        None = 0,

    
    }

    public enum MainSceneRedDotType : byte
    {
        None = 0,

        Mail,      
        Mission,
        Friend,
        Store,
        VIP,
    }

    public enum OSType : byte
    {
        None,
        Android,
        IOS,
    }

    public enum StoreProductType : byte
    {
        None = 0,
        Gacha = 1,
        Package = 2,
        PackageSpecial = 3,
    }

    public enum CouponType : byte
    {
        None = 0,

        All = 1,   // 쿠폰 코드가 한계정에만 사용 가능 -> 1번 유저가 AAA 쿠폰 사용하면 2번 유저는 AAA 쿠폰 사용 못함
        User = 2,   // 모든 계정이 사용 가능 하지만 같은 계정은 한번만 사용 가능 
    }

    public enum StoreType : byte
    {
        None = 0,
        GooglePlay = 1,
        AppleAppStore = 2,
    }

    public enum AccountType : byte
    {
        None = 0,
        Guest,
        GPGS,
        GameCenter,
        Google,
        Apple,
    }

    public enum GachaType : byte
    {
        None = 0,
        Rare = 1,
        TimeLimit = 2,
        Special = 3,
    }


    public enum AttendanceType : byte
    {
        None = 0,

        Normal = 1,
        Newbie = 2,
        Return = 3, 
        
    }

  
    public enum GradeType : byte
    {
        None = 0,
        Normal = 1,
        Rare = 2,
        Unique = 3,
        Epic = 4,
        Legendary = 5,
    }


    public enum Cond1Type : byte
    {
        None,

        Login,
        Craft,
        Disassembly,
        Processing,
        Obtain,
        Consume,
        LevelUp,
        Level,
        GradeUpgrade,
        Grade,
        PlayTime,
        Advertisement,
        Contest,
        Friend,
        Recommend,
        Sale,
        Profile,        
        SoldOut,
        Catalog,
        Sell,
        Clear,
        
    }

    public enum Cond2Type : byte
    {
        None,

        All,
        Hair,
        Top,
        Bottom,
        Dress,
        Socks,
        Shoes,
        HeadWear,
        HairPin,
        FaceAccessories,
        Earings,
        NeckAccessories,
        LegAccessories,
        Gloves,
        ArmAccessories,
        UpperBodyAccessories,
        HandProps,
        SpecialProps,
        Parts,
        Material,
        Currency,
        ShoppingMall,
        Stats,
        Cashier,
        SeedGold,
        PerCritical,
        IncDropMaterial,
        IncClothesSale,
        IncCatalogueSale,
        IncCatalogueGold,
        IncFastCustomer,
        IncGoldenCustomer,
        IncMerchantCustomer,
        IncCelebCustomer,
        IncDropMaterialR,
        IncDropMaterialU,
        PerXCritical,
        Cashier01,
        Cashier02,
        Cashier03,
        Cashier04,
        Cashier05,
        ShoppingmallGrade,
        Hour,
        Reward,
        Participation,
        Rank,
        RankUp,        
        Send,
        Making,
        My,
        Staff,
        Posting,        
        Comment,
        Leaflet,
        Save,
        Like,
        Item_Intimacy,
        SponsorshipCharacter,
        MissionClear,
        Donation,
        SetUp,
        BuffChange
    }

    public enum FriendState : byte
    {
        None, 

        Friend,
        Request,
    }

    public enum BoxType : byte
    {
        None, 

        Random,
        Fix, 
        Select,
    }
    

    public enum StoreBadgeType : byte
    {
        None, 

        New,
        Hot,
        Best,
        Event,
    }


    public enum EventType : byte
    {
        None,
        HotTime,
        Mission,
        Exchange
    }
   
    public enum NewUserConfigType : byte
    {
        None,
        
        Off,
        On,
        HalfOn,
    }

    public enum SpecialBuffType : byte
    {
        None,
        AutoOperation,
        DeletAds,
    }

    public enum VipBuffType : byte
    {
        None,

        vip_add_sale_gold,              // 의상 1개 판매 시 더해지는 추가 골드
        vip_inc_sale_gold,              // 의상 판매로 얻는 골드 수익의 추가 골드 %
        vip_inc_sale_ct_gold,           // 카탈로그 판매 시 증가하는 추가 골드 %
        vip_add_heart,                  // 하트 1회 획득 당 추가되는 하트 수량
        vip_add_ct_coin,                // 튤립 1회 획득 당 추가되는 튤립 수량
        vip_add_customer_limit,         // 손님 제한 수 상승
        vip_add_leaflet_max,            // 일반 전단지 최대 개수 상승
        vip_deduct_leaflet_touch,       // 전단지 터치 횟수 차감
        vip_add_cleanliness_max,        // 청결도 최대치 상승
        vip_add_offlinegift,            // 오프라인 보상 지급 시 추가 지급 개수 %
        vip_deduct_angryautotouchtime,  // 진상 손님 자동 퇴치 시간 감소 %
        vip_add_comment,                // 하루 댓글 등록 개수 상향
    }
    public enum VipShopType : byte
    {
        None,
        PartsShop,
        ItemShop,

    }
    //현재 유저 세션상태 위치(이름은 추후 수정을 합시다)
    public enum SessionLocationType
    {
        None,
        Disconnected,   // 세션서버 접속안되어있음..
        Frontend,       // 기본적인 세션로그인 이후
        Lobby,          // EnterMainLobby이후
        Channel,        // EnterChannel이후
        GameRoom,       // EnterGameRoom이후
        
    }

}
