using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shared.Clock;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class StorePackageSpecialCSVData : BaseCSVData
    {
        public override string GetFileName() => "Store_Package_Special.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("product_order")]
        public int ProductOrder { get; private set; }

        [CSVColumn("fr_dt")]
        public DateTime FrDt { get; private set; }

        [CSVColumn("to_dt")]
        public DateTime ToDt { get; private set; }

        public DateTime ToMaxPurchaseDt { get; private set; }

        [CSVColumn("purchase_time_min")]
        public int PurchaseTimeMin { get; private set; }

        [CSVColumn("package_view_value")]
        public long PackageViewValue { get; private set; }

        [CSVColumn("user_attendance_type")]
        public AttendanceType AttendanceType { get; private set; }

        [CSVColumn("fr_level")]
        public int FrLevel { get; private set; }

        [CSVColumn("to_level")]
        public int ToLevel { get; private set; }

        [CSVColumn("fr_inapp")]
        public int FrInApp { get; private set; }

        [CSVColumn("to_inapp")]
        public int ToInApp { get; private set; }

        [CSVColumn("server_total_limit_count")]
        public int serverTotalLimitCount { get; private set; }

        [CSVColumn("badge")]
        public StoreBadgeType BadgeType { get; private set; }

        [CSVColumn("limit_count_cycle")]
        public CycleType CycleType { get; private set; }

        [CSVColumn("purchase_limit_count")]
        public int PurchaseLimitCount { get; private set; }

        [CSVColumn("aos_store_product_id")]
        public string AosProductId { get; private set; }

        [CSVColumn("ios_store_product_id")]
        public string IosProductId { get; private set; }

        [CSVColumn("payment_type")]
        public RewardPaymentType PaymentType { get; private set; }

        [CSVColumn("payment_index")]
        public long PaymentIndex { get; private set; }

        [CSVColumn("payment_amount")]
        public BigInteger PaymentAmount { get; private set; }

        [CSVColumn("continue_reward")]
        public bool ContinueReward { get; private set; }

        [CSVColumn("continue_reward_num")]
        public int ContinueRewardNum { get; private set; }

        [CSVColumn("continue_reward_term")]
        public int ContinueRewardTerm { get; private set; }

        [CSVColumn("reward_group_id")] 
        public long RewardGroupId { get; private set; }

        [CSVColumn("language_title")]
        public string LanguageTitle { get; private set; }

        [CSVColumn("ui_theme")]
        public string UiTheme { get; private set; }

        [CSVColumn("background_theme")]
        public string BackgroundTheme { get; private set; }

        [CSVColumn("set_guide_thumbnail")]
        public string SetGuideThumbnail { get; private set; }

        [CSVColumn("sale_percentage")]
        public int SalePercentage { get; private set; }

        public bool ServerTotalSoldOut { get; private set; }

        public bool IsValidTime(DateTime now) => (FrDt <= now && now <= ToMaxPurchaseDt);

        public bool IsValidLevel(int level) => (FrLevel <= level && level <= ToLevel);

        public bool IsValidBuyInApp(long buyInApp) => (FrInApp <= buyInApp && buyInApp <= ToInApp);

        public bool IsFree { get; private set; }

        public List<ItemInfo> RewardInfos { get; private set; } = new List<ItemInfo>();

        public override void Init()
        {
            base.Init();
            ToMaxPurchaseDt = ToDt;
            if(ToMaxPurchaseDt < AppClock.MaxValue)
            {
                ToMaxPurchaseDt = ToDt.AddMinutes(PurchaseTimeMin);
            }
            
            IsFree = (PaymentType == RewardPaymentType.Currency && PaymentIndex == (long)CurrencyType.Free);
            ServerTotalSoldOut = false;

            var setGuideThumbnail = long.Parse(SetGuideThumbnail);
            SetGuideThumbnail = $"{setGuideThumbnail:D4}";
        }

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            RewardInfos = csvData.StorePackageSpecialRewardGroupListData.Where(p => p.RewardGroupId == RewardGroupId).Select(p=>p.RewardInfo).ToList();  
        }

        
        
}
}
