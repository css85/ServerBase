using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class PointCSVData : BaseCSVData
    {
        public override string GetFileName() => "Point.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("point_type")]
        public PointType PointType { get; private set; }

    }

}
