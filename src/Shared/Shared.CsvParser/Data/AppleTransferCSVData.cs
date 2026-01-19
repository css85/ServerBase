using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class AppleTransferCSVData : BaseCSVData
    {
        public override string GetFileName() => "AppleTransfer.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("before_account_id")]
        public string BeforeAccountId { get; private set; }

        [CSVColumn("after_account_id")]
        public string AfterAccountId { get; private set; }

    }

}
