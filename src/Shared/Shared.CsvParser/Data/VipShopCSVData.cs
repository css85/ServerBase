using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class VipShopCSVData : BaseCSVData
    {
        public override string GetFileName() => "VIP_Shop.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("category_order")]
        public int CategoryOrder { get; private set; }

        [CSVColumn("vip_shop_type")]
        public VipShopType VipShopType { get; private set; }

        [CSVColumn("product_group_id")]
        public long ProductGroupId { get; private set; }

        public List<VipShopPartsRewardGroupCSVData> ShopPartsRewardGroups { get; private set; } = new List<VipShopPartsRewardGroupCSVData>();
        public List<VipShopItemRewardGroupCSVData> ShopItemRewardGroups { get; private set; } = new List<VipShopItemRewardGroupCSVData>();
        

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            if( VipShopType == VipShopType.PartsShop)
                ShopPartsRewardGroups = csvData.VipShopPartsRewardGroupListData.Where(p => p.ProductGroupId == ProductGroupId).ToList();
            else if( VipShopType == VipShopType.ItemShop)
                ShopItemRewardGroups = csvData.VipShopItemRewardGroupListData.Where(p => p.ProductGroupId == ProductGroupId).ToList();


        }

    }

}
