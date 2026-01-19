using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.ServerApp.Services;
using Shared.Server.Define;
using Shared.Repository.Services;
using StackExchange.Redis;
using WebTool.Base.DataTables;
using WebTool.Connection.Services;
using static Shared.Session.Extensions.ReplyExtensions;
using Shared.Server.Packet.Internal;
using Shared;
using Shared.Services.Redis;

namespace WebTool.Controllers
{
    [Route("api/server")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly ILogger<ServerController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabaseAsync _sessionRedis;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly ServerSessionService _serverSesionService;
        private readonly CsvStoreContext _csvStoreContext;

        public ServerController(
            ILogger<ServerController> logger,
            IServiceProvider serviceProvider,
            CsvStoreContext csvStoreContext,
            DatabaseRepositoryService dbRepo,
            RedisRepositoryService redisRepo,
            ServerSessionService serverSessionService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _sessionRedis = redisRepo.GetDb(RedisDatabase.Session);
            _dbRepo = dbRepo;
            _csvStoreContext = csvStoreContext;
            _serverSesionService = serverSessionService;
        }


        [HttpPost("regist-inspector")]
        public async Task<IActionResult> SetInspectorServer()
        {
            if (!Request.Form.TryGetValue("OsType", out var osTypeText))
            {
                return BadRequest("올바르지 않은 OS 형식입니다.");
            }
            if (!byte.TryParse(osTypeText, out var osType))
            {
                return BadRequest("올바르지 않은 OS 형식입니다.");
            }
            string version = Request.Form["Version"].FirstOrDefault();
            if (string.IsNullOrEmpty(version))
            {
                return BadRequest("버전 정보가 없습니다.");
            }
            if (!Request.Form.TryGetValue("Flag", out var flagText))
            {
                return BadRequest("올바른 설정값이 아닙니다.");
            }
            if (!bool.TryParse(flagText, out var isDelete))
            {
                return BadRequest("올바른 설정값이 아닙니다.");
            }

            using var gateCtx = _dbRepo.GetGateDb();

         
            await gateCtx.SaveChangesAsync();
            await _serverSesionService.SendAllAsync(
                NetServiceType.Gate,
                MakeNtfReply(new InternalRefreshServerNtf()
                {
                }));

            await _serverSesionService.SendAllAsync(
                NetServiceType.FrontEnd,
                MakeNtfReply(new InternalRefreshServerNtf()
                {
                }));

            return Ok();
        }

        
        public static DataTablesColumnInfo[] WhiteListInfo =
        {
            new DataTablesColumnInfo("유저 번호", ""),
        };

        public static DataTablesColumnInfo[] BundleInfo =
        {
            new DataTablesColumnInfo("OS", ""),
            new DataTablesColumnInfo("번들 버전", ""),
            new DataTablesColumnInfo("번들 URL", ""),
        };

     

        public static DataTablesColumnInfo[] GetNtfLogInfo =
        {
            new DataTablesColumnInfo("등록 번호"),
            new DataTablesColumnInfo("내용"),
            new DataTablesColumnInfo("예약 일시"),
            new DataTablesColumnInfo("등록 일시"),
            new DataTablesColumnInfo("예약 취소"),
        };



        public static DataTablesColumnInfo[] HotTimeBuffInfos =
        {
            new DataTablesColumnInfo("버프 타입"),
            new DataTablesColumnInfo(""),
        };

        public static DataTablesColumnInfo[] RegistHotTimeBuffInfos =
        {
            new DataTablesColumnInfo("담당자"),
            new DataTablesColumnInfo("조작 일시"),
            new DataTablesColumnInfo("시작 일시"),
            new DataTablesColumnInfo("종료 일시"),
            new DataTablesColumnInfo("버프 타입"),
        };


        [HttpPost("set-maintenance-time")]
        public async Task<IActionResult> SetMaintenanceTime()
        {
            if (!Request.Form.TryGetValue("StartTime", out var startTimeText))
            {
                return BadRequest("올바르지 않은 설정값입니다.");
            }
            if (!DateTime.TryParse(startTimeText, out var startTime))
            {
                return BadRequest("올바르지 않은 설정값입니다.");
            }
            if (!Request.Form.TryGetValue("EndTime", out var endTimeText))
            {
                return BadRequest("올바르지 않은 설정값입니다.");
            }
            if (!DateTime.TryParse(endTimeText, out var endTime))
            {
                return BadRequest("올바르지 않은 설정값입니다.");
            }
            using var gateCtx = _dbRepo.GetGateDb();

            //var appData = await gateCtx.AppGroups.FindAsync(1);

            //if (appData == null)
            //{
            //    return BadRequest("존재하지 않은 게이트입니다.");
            //}

            //appData.IsMaintenance = true;
            //appData.MaintenanceFromTime = startTime;
            //appData.MaintenanceToTime = endTime;

            //gateCtx.AppGroups.Update(appData);
            await gateCtx.SaveChangesAsync();

            return Ok();
        }

        public static DataTablesColumnInfo[] MaintenanceTimeTable =
        {
            new DataTablesColumnInfo("Gate", ""),
            new DataTablesColumnInfo("Start", ""),
            new DataTablesColumnInfo("End", ""),
        };

      
    }
}
