using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminServer.Services;
using Common.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.PacketModel;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using Shared.Services.Redis;
using StackExchange.Redis;

namespace AdminServer.Controllers
{
    [Route("admin")]
    public class AdminController : BaseApiController
    {
        private readonly ILogger<AdminController> _logger;
        private readonly CsvStoreContext _csvContext;
        private readonly PublishService _publishService;
        private readonly RedisRepositoryService _redisRepo;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly CommandService _commandService;
        private readonly RankingScheduleService _rankingScheduleService;

        public AdminController(
            ILogger<AdminController> logger,
            ChangeableSettings<GameRuleSettings> gameRule,
            CsvStoreContext csvContext,
            PublishService publishService,
            RedisRepositoryService redisRepo,
            DatabaseRepositoryService dbRepo,
            CommandService commandService,
            RankingScheduleService rankingScheduleService,
            AdminScheduleService adminScheduleService
            )
        {
            _logger = logger;
            _csvContext = csvContext;
            _publishService = publishService;   
            _redisRepo = redisRepo; 
            _dbRepo = dbRepo;
            _commandService = commandService;
            _rankingScheduleService = rankingScheduleService;
        }

        /// <summary>
        /// 
        /// </summary>
        [AllowAnonymous]
        [HttpPost("command")]
        public async Task<ActionResult<AdminCommandRes>> ServerCommand([FromBody] AdminCommandReq req)
        {
            var csvData = _csvContext.GetData();
            using var userCtx = _dbRepo.GetUserDb();
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
//            HttpContext.Request.Headers.Authorization.FirstOrDefault();
            var result1 = "";
            if (req.Command == "csv")
            {
                await _publishService.PublishCsvAsync();
                _csvContext.GetData().LoadCsvDataAll();
                
            }
            else if( req.Command == "connect-user")
            {
                var connectCount = _redisRepo.GetKeysCount(RedisDatabase.User, "UserConnect:*");
                result1 = connectCount.ToString();                
            }
            else if( req.Command == "cache_refresh")
            {
                //if( req.Option1 == "likebest_rank")
                //{
                //    await _publishService.PublishLikeBestAsync();
                //    await _rankingScheduleService.SetLikeBestInfoAsync();
                //}
            }
            else if ( req.Command == "redis_reload")
            {
                if (req.Option1 == "shoppingmall_rank")
                    await _commandService.ReloadShoppingmallRankAsync();
                
            }
            else if(req.Command == "apple_transfer")
            {
//                await _commandService.AppleLoginTransferAsync(req.Option1);
            }

            return Ok(ResultCode.Success, new AdminCommandRes { Result1 = result1});
        }
    }
}
