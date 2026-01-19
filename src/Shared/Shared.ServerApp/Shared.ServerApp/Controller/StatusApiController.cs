using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Connection;

namespace Shared.ServerApp.Controller
{
    [Route("_status")]
    public class StatusApiController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<StatusApiController> _logger;
        private readonly AppServerSessionServiceBase _appServerSessionService;

        public StatusApiController(
            ILogger<StatusApiController> logger,
            AppServerSessionServiceBase appServerSessionService
            )
        {
            _logger = logger;
            _appServerSessionService = appServerSessionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // 유저 카운팅

            // 레디스 상태 확인

            // DB 상태 확인

            // LogInformation으로 출력

            // Json으로 결과

            return Ok();
        }
    }
}