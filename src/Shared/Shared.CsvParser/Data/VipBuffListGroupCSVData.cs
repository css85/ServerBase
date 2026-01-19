using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class VipBuffListGroupCSVData : BaseCSVData
    {
        public override string GetFileName() => "VIP_BuffListGroup.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("vipbuff_group_id")]
        public long VipBuffGroupId { get; private set; }

        [CSVColumn("vipbuff_type")]
        public VipBuffType VipBuffType { get; private set; }

        [CSVColumn("value")]
        public int Value { get; private set; }

    }

}
