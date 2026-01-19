using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class ItemIntimacyCSVData : BaseCSVData
    {
        public override string GetFileName() => "Item_Intimacy.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("Intimacy_add")]
        public int IntimacyAdd { get; private set; }

    }

}
