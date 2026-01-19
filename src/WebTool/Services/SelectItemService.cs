using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.CsvData;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using WebTool.Base.Item;

namespace WebTool.Services
{
    public class SelectItemService
    {
        private readonly ILogger<SelectItemService> _logger;
        private readonly CsvStoreContext _csvContext;
        
        private SelectItemType[] _categories;
        private Dictionary<SelectItemType, SelectItemData[]> _selectItemMap = new();

        public SelectItemService(
            ILogger<SelectItemService> logger,
            CsvStoreContext csvStoreContext
        )
        {
            _logger = logger;
            _csvContext = csvStoreContext;

            Init();
        }

        public void Init()
        {
            var csvData = _csvContext.GetData();

            _categories = Enum.GetValues<SelectItemType>().Where(p => p != SelectItemType.None && (int) p % 100 > 0)
                .OrderBy(p => (int) p)
                .ToArray();

            var items = new List<SelectItemData>();
            {
                items.Add(new SelectItemData()
                {
                    Type = SelectItemType.Ruby,
                    Index = 0,
                    Name = "루비",
                });
                items.Add(new SelectItemData()
                {
                    Type = SelectItemType.Den,
                    Index = 0,
                    Name = "덴",
                });
                //items.AddRange(AddCsvItems(SelectItemType.Item_Global_Material,
                //    csvData.AllItemData[CsvItemType.Item_Global_Material].Select(p => (p.Key, p.Value.Item2))));
                
                //items.AddRange(AddCsvItems(SelectItemType.Item_DanceMaster_Jewel,
                //    csvData.AllItemData[CsvItemType.Item_DanceMaster_Jewel].Select(p => (p.Key, p.Value.Item2))));
                //items.AddRange(AddCsvItems(SelectItemType.Item_Collection_Material,
                //    csvData.AllItemData[CsvItemType.Item_Collection_Material].Select(p => (p.Key, p.Value.Item2))));

            }

            var itemMap = new Dictionary<SelectItemType, List<SelectItemData>>();
            {
                foreach (var item in items)
                {
                    if (itemMap.TryGetValue(item.Type, out var itemList) == false)
                    {
                        itemList = new List<SelectItemData>();
                        itemMap.Add(item.Type, itemList);
                    }

                    itemList.Add(item);
                }
            }

            _selectItemMap = itemMap.ToDictionary(p => p.Key, p => p.Value.ToArray());
        }

        private dynamic AddCsvItems(SelectItemType type, IEnumerable<(long, string)> values)
        {
            return values.Select(p => new SelectItemData
            {
                Type = type,
                Index = p.Item1,
                
            }).OrderBy(p => p.Index);
        }

        private dynamic AddCsvItems(SelectItemType type, IEnumerable<(long, string, string)> values)
        {
            return values.Select(p => new SelectItemData
            {
                Type = type,
                Index = p.Item1,
                
            }).OrderBy(p => p.Index);
        }

        public SelectItemData[] GetData(SelectItemType type)
        {
            return _selectItemMap[type];
        }

        public SelectItemType[] GetCategories()
        {
            return _categories;
        }

      
    }
}
