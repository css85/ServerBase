using System;
using System.Collections.Generic;
using System.Net;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// 상점 입장
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Store, (byte)STORE_MINOR.EnterStore, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/store/enter")]
    public class EnterStoreReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class EnterStoreRes : ResponseBase
    {
        public List<GachaStoreInfo> GachaStoreInfo { get; set; } = new List<GachaStoreInfo>();     // 가챠 상점 정보들
        public List<PackageStoreInfo> PackageInfo { get; set; } = new List<PackageStoreInfo>();     // 패키지 상점 정보들
    }

   
    /// <summary>
    /// 가챠 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Store, (byte)STORE_MINOR.BuyGacha, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/store/buygacha")]
    public class BuyGachaReq : RequestBase
    {
        public long StoreGachaIndex { get; set; }
        public int BuyCount { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-1301"> NotSellProduct 구매할 수 없는 상품  </response>
    /// <response code="-1303"> NotSellTime 판매시간이 아님  </response>
    [Serializable]
    public class BuyGachaRes : ResponseBase
    {
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

    /// <summary>
    /// 패키지 구매
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Store, (byte)STORE_MINOR.BuyPackage, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/store/buypackage")]
    public class BuyPackageReq : RequestBase
    {
        public long StorePackageIndex { get; set; }
        public int BuyCount { get; set; }
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-1301"> NotSellProduct 구매할 수 없는 상품  </response>
    /// <response code="-1302"> StoreLimit 제한 갯수가 모자름  </response>
    [Serializable]
    public class BuyPackageRes : ResponseBase
    {
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
        public PackageStoreInfo RefreshPackageInfo { get; set; } = new PackageStoreInfo();
    }


   

    /// <summary>
    /// 인앱 상품 구매전 체크
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Store, (byte)STORE_MINOR.BuyInAppCheck, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/store/buyinapp-check")]
    public class BuyInAppCheckReq : RequestBase
    {
        public StoreProductType StoreProductType { get; set; }
        public long Index { get; set; }
        public string Option1 { get; set; }
    }
    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    
    [Serializable]
    public class BuyInAppCheckRes : ResponseBase
    {
        
    }

    /// <summary>
    /// 인앱 재화 구매
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Store, (byte)STORE_MINOR.BuyInApp, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, httpPostPath: "/api/store/buyinapp")]
    public class BuyInAppReq : RequestBase
    {
        public StoreProductType StoreProductType { get; set; }
        public PurchaseData PurchaseData { get; set; }
    }
    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    // -- 재화 또는 아이템 소비 부분 공통 에러 코드 
    /// <response code="-110"> NotEnoughGold 골드 부족   </response>
    /// <response code="-111"> NotEnoughSIDO 시도코인 부족   </response>
    /// <response code="-112"> NotEnoughGameCoin MFL 코인 부족   </response>
    /// <response code="-113"> NotEnoughItem 아이템 부족   </response>
    [Serializable]
    public class BuyInAppRes : ResponseBase
    {
        public StoreProductType StoreProductType { get; set; }
        public long StoreIndex { get; set; }
        public long BuyInAppCount { get; set; }
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshInventoryInfo RefreshInfo { get; set; } = new RefreshInventoryInfo();
    }

}
