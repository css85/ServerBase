using System;
using GateServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.PacketModel;
using Shared.Server.Define;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;

namespace GateServer.Controllers
{
    [Route("gate")]
    public class GateController : BaseApiController
    {
        private readonly ILogger<GateController> _logger;
        private readonly CsvStoreContext _csvContext;
        private readonly GateCheckService _gateCheckService;

        public GateController(
            ILogger<GateController> logger,
            CsvStoreContext csvContext,
            GateCheckService gateCheckService
            )
        {
            _logger = logger;
            _csvContext = csvContext;
            _gateCheckService = gateCheckService;   
        }

        /// <summary>
        /// ostype 앱버전에 따른 서버선택
        /// </summary>
        [AllowAnonymous]
        [HttpPost("get-servers")]
        public ActionResult<GetServerInfosRes> GetServerInfos([FromBody] GetServerInfosReq req)
        {

            if (req.OsType == (byte)OSType.None)
                return Ok<GetServerInfosRes>(ResultCode.InvalidParameter);
            if (Enum.IsDefined(typeof(OSType), req.OsType) == false)
                return Ok<GetServerInfosRes>(ResultCode.InvalidParameter);

            var serverLocationType = _gateCheckService.GetLocationType(new Tuple<int, OSType>(req.AppVer, req.OsType));
            if (serverLocationType == ServerLocationType.None)
                return Ok<GetServerInfosRes>(ResultCode.NotSupportVersion);

            if(_gateCheckService.CheckServerAllowCountry(GetIP(), serverLocationType) == false)
                return Ok<GetServerInfosRes>(ResultCode.BlockCountry);

//            var ContentConfigs = _gateCheckService.GetContentConfigs(req.OsType, serverLocationType);
            long inspectionTick = _gateCheckService.CheckServerInspection(GetIP(), serverLocationType);
            if (inspectionTick > 0)
            {
                return Ok(ResultCode.Success, new GetServerInfosRes
                {
                    InspectionToDtTick = inspectionTick,
                    IsServerInspection = true,
                    ServerTimeTick = AppClock.UtcNow.Ticks,
//                    ContentConfigs = ContentConfigs,
                });
            }

            var serverInfos = _gateCheckService.GetServerInfo(serverLocationType);
            
            return Ok(0, new GetServerInfosRes
            {
                InspectionToDtTick = 0,
                IsServerInspection = false,
                ServerTimeTick = AppClock.UtcNow.Ticks,
                ServiceServers = serverInfos,
//                ContentConfigs = ContentConfigs,
                AssetPatchUrl = _gateCheckService.GetCdnInfo(serverLocationType),
            });

        }

    }
}

