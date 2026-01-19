using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class MaterialCSVData : BaseCSVData
    {
        public override string GetFileName() => "Material.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("material_combine")]
        public bool MaterialCombine { get; private set; }

        [CSVColumn("material_grade")]
        public GradeType GradeType { get; private set; }

        [CSVColumn("language_key")]
        public string LanguageKey { get; private set; }

        public MaterialCombineCSVData MaterialCraftData { get; private set; }


        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);
            MaterialCraftData = csvData.MaterialCombineListData.Where(p=>p.OriginalMaterialIndex == Index).FirstOrDefault();
        }
    }

}
