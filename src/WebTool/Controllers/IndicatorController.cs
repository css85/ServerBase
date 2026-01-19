using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Repository.Services;
using Shared.ServerApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebTool.Base.DataTables;
using Shared.Server.Define;
using Nest;

namespace WebTool.Controllers
{
    [Route("api/indicator")]
    [ApiController]
    public class IndicatorController : ControllerBase
    {
        private readonly ILogger<IndicatorController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DatabaseRepositoryService _dbRepo;        
        private readonly CsvStoreContext _csvStoreContext;
        public IndicatorController(
            ILogger<IndicatorController> logger,
            IServiceProvider serviceProvider,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvStoreContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _dbRepo = dbRepo;
            _csvStoreContext = csvStoreContext;
        }

        public static DataTablesColumnInfo[] PurchaseItemLog =
        {
            new DataTablesColumnInfo("아이템 이름", ""),
            new DataTablesColumnInfo("구매량", ""),
        };

        [HttpPost("purchase-item-log")]
        public IActionResult GetPurchaseItemSingleDayLog()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, PurchaseItemLog);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);
            
            if (!Request.Form.TryGetValue("FromDt", out var startDateText))
            {
                return BadRequest("올바르지 않은 필터값입닌다.");
            }
            if (!DateTime.TryParse(startDateText, out var startDate))
            {
                return BadRequest("올바르지 않은 필터값입니다.");
            }
            if (!Request.Form.TryGetValue("ToDt", out var endDateText))
            {
                return BadRequest("올바르지 않은 필터값입니다.");
            }
            if (!DateTime.TryParse(endDateText, out var endDate))
            {
                return BadRequest("올바르지 않은 필터값입니다.");
            }

            var itemDic = _csvStoreContext.GetData().AllItemData;

            //아이템 정보 다 긁어오기

            var itemLogDic = new Dictionary<string, int>();
            //아이템 정보 저장하기
            

            dataTablesOutput.data = itemLogDic.Select(p => new[]
            {
                p.Key,
                p.Value.ToString(),
            }).ToArray();

            return Ok(dataTablesOutput);
        }
    }
}
