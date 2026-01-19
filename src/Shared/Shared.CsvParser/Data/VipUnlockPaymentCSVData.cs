using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class VipUnlockPaymentCSVData : BaseCSVData
    {
        public override string GetFileName() => "VIP_Unlock_Payment.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("view")]
        public bool View { get; private set; }

        [CSVColumn("buff_time_min")]
        public int BuffTimeMin { get; private set; }

        [CSVColumn("payment_type")]
        public RewardPaymentType PaymentType { get; private set; }

        [CSVColumn("payment_index")]
        public long PaymentIndex { get; private set; }

        [CSVColumn("payment_amount")]
        public int PaymentAmount { get; private set; }

        public ItemInfo PaymentInfo { get; private set; }

        public override void Init()
        {
            base.Init();
            PaymentInfo = new ItemInfo(PaymentType, PaymentIndex, PaymentAmount);
        }

    }

}
