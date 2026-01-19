using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Services;
using WebTool.Base.DataTables;
using WebTool.Base.Item;
using WebTool.Extensions;
using WebTool.Services;

namespace WebTool.Controllers
{
    [Route("api/select-item")]
    [ApiController]
    public class SelectItemController : ControllerBase
    {
        private readonly ILogger<SelectItemController> _logger;
        private readonly CsvStoreContext _csvContext;
        private readonly SelectItemService _selectItemService;

        public SelectItemController(
            ILogger<SelectItemController> logger,
            CsvStoreContext csvContext,
            SelectItemService selectItemService
            )
        {
            _logger = logger;
            _csvContext = csvContext;
            _selectItemService = selectItemService;
        }

        public static DataTablesColumnInfo[] DefaultTableDataColumnInfos =
        {
            new DataTablesColumnInfo("Id", "Index"),
            new DataTablesColumnInfo("이름", "Name"),
            new DataTablesColumnInfo("선택"),
        };

        [HttpPost("item-table-data")]
        public IActionResult ItemTableData()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, DefaultTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            var itemType = Enum.Parse<SelectItemType>(Request.Form.GetFormValue<string>("type"));

            var items = _selectItemService.GetData(itemType);

            dataTablesOutput.recordsTotal = dataTablesOutput.recordsFiltered = items.Length;

            var enumerableItems = items.AsEnumerable();

            foreach (var searchValue in dataTablesInput.searchValues)
            {
                if (string.IsNullOrEmpty(searchValue) == false)
                    enumerableItems = enumerableItems.Where(p => p.Name.Contains(searchValue));
            }

            var orderedCollection = dataTablesInput.ApplyOrder(enumerableItems);

            dataTablesOutput.recordsFiltered = orderedCollection.Count();

            enumerableItems = dataTablesInput.ApplyLimit(orderedCollection);
        
            dataTablesOutput.data = enumerableItems.Select(p => new[]
            {
                $"{p.Index}",
                $"{p.Name}",
                "",
            }).ToArray();

            return Ok(dataTablesOutput);
        }

        public static DataTablesColumnInfo[] CostumeTableDataColumnInfos =
        {
            new DataTablesColumnInfo("Id", "Index"),
            new DataTablesColumnInfo("성별", "Gender"),
            new DataTablesColumnInfo("타입", "CostumeType"),
            new DataTablesColumnInfo("슬롯 타입", "CostumeType"),
            new DataTablesColumnInfo("등급", "CostumeGrade"),
            new DataTablesColumnInfo("상점 노출", "StoreView"),
            new DataTablesColumnInfo("구매 재화", "PriceType"),
            new DataTablesColumnInfo("가격", "Price"),
            new DataTablesColumnInfo("이름", "Name"),
            new DataTablesColumnInfo("선택"),
        };

        [HttpPost("costume-table-data")]
        public IActionResult CostumeTableData()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, CostumeTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            var itemType = Enum.Parse<SelectItemType>(Request.Form.GetFormValue<string>("type"));

            var items = _selectItemService.GetData(itemType);

            dataTablesOutput.recordsTotal = dataTablesOutput.recordsFiltered = items.Length;

            var enumerableItems = items.AsEnumerable().Select(p => (CostumeSelectItemData) p);

            foreach (var searchValue in dataTablesInput.searchValues)
            {
                if (string.IsNullOrEmpty(searchValue) == false)
                    enumerableItems = enumerableItems.Where(p => p.Name.Contains(searchValue));
            }

            var orderedCollection = dataTablesInput.ApplyOrder(enumerableItems);

            dataTablesOutput.recordsFiltered = orderedCollection.Count;

            enumerableItems = dataTablesInput.ApplyLimit(orderedCollection);

            dataTablesOutput.data = enumerableItems.Select(p => new[]
            {
                $"{p.Index}",
                $"{p.Gender}",
                $"{p.CostumeType}",
                $"{string.Join(',', p.CostumeSlotTypes)}",
                $"{p.CostumeGrade}",
                $"{p.StoreView}",
                $"{p.PriceType}",
                $"{p.Price}",
                $"{p.Name}",
                "",
            }).ToArray();

            return Ok(dataTablesOutput);
        }
    }
}