using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class SpecialBuffCSVData : BaseCSVData
    {
        public override string GetFileName() => "SpecialBuff.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }
        
        [CSVColumn("special_buff_type")]
        public SpecialBuffType SpecialBuffType { get; private set; }


    }

}
