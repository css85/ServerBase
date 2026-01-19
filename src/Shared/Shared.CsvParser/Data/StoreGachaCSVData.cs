using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class StoreGachaCSVData : BaseCSVData
    {
        public override string GetFileName() => "Store_Gacha.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("fr_dt")]
        public DateTime FrDt { get; private set; }

        [CSVColumn("to_dt")]
        public DateTime ToDt { get; private set; }

        [CSVColumn("gacha_type")]
        public GachaType GachaType { get; private set; }

        [CSVColumn("payment_type_1")]
        public RewardPaymentType PaymentType_1 { get; private set; }

        [CSVColumn("payment_index_1")]
        public long PaymentIndex_1 { get; private set; }

        [CSVColumn("payment_amount_1")]
        public long PaymentAmount_1 { get; private set; }

        [CSVColumn("payment_type_2")]
        public RewardPaymentType PaymentType_2 { get; private set; }

        [CSVColumn("payment_index_2")]
        public long PaymentIndex_2 { get; private set; }

        [CSVColumn("payment_amount_2")]
        public long PaymentAmount_2 { get; private set; }

        [CSVColumn("payment_2_discount_rate_1")]
        public long PaymentDiscountRate_2_1 { get; private set; }

        [CSVColumn("payment_2_discount_rate_10")]
        public long PaymentDiscountRate_2_10 { get; private set; }

        [CSVColumn("prob_group_id")] 
        public long ProbGroupId { get; private set; }

        public List<StoreGachaProbGroupCSVData> ProbGroups { get; private set; } = new List<StoreGachaProbGroupCSVData>();
        public Dictionary<StoreGachaProbGroupCSVData, long> ProbGroupData { get; private set; }

        public List<StoreGachaPickupCSVData> PickupDatas { get; private set; } = new List<StoreGachaPickupCSVData>();
        public bool IsValidTime(DateTime now)
        {
            return (FrDt <= now && now < ToDt);
        }
        public long GetPaymentAmount2(int buyCount)
        {   
            if (buyCount == 1)
                return (long)Math.Truncate(PaymentAmount_2 * (1 - (PaymentDiscountRate_2_1 * 0.01)));
            else if( buyCount == 10)
                return (long)Math.Truncate(PaymentAmount_2 * (1 - (PaymentDiscountRate_2_10 * 0.01)));
            return PaymentAmount_2;
        }
        

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);

            ProbGroups = csvData.StoreGachaProbGroupListData.Where(p=>p.ProbGroupId == ProbGroupId).ToList();
            ProbGroupData = ProbGroups.Select(p => KeyValuePair.Create(p, p.ProbWeight)).ToDictionary(p => p.Key, p => p.Value);
            PickupDatas = csvData.StoreGachaPickupListData.Where(p => p.GachaIndex == Index).ToList();
        }

        
        
}
}
