using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class MaterialCombineCSVData : BaseCSVData
    {
        public override string GetFileName() => "Material_Combine.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("original_material_index")]
        public long OriginalMaterialIndex { get; private set; }

        [CSVColumn("original_material_amount")]
        public int OriginalMaterialAmount { get; private set; }

        [CSVColumn("target_material_index")]
        public long TargetMaterialIndex { get; private set; }
    }

}
