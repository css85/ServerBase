using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class CurrencyCSVData : BaseCSVData
    {
        public override string GetFileName() => "Currency.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("currency_type")]
        public CurrencyType CurrencyType { get; private set; }

        [CSVColumn("charge_max")]
        public long ChargeMax { get; private set; }

    }

}
