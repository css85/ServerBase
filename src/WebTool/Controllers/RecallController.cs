using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Services;
using Shared.Repository.Services;
using WebTool.Connection.Services;

using static Shared.Session.Extensions.ReplyExtensions;

namespace WebTool.Controllers
{
    [Route("api/recall")]
    [ApiController]
    public class RecallController : ControllerBase
    {
        private readonly ILogger<RecallController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ServerSessionService _serverSessionService;
        private readonly DatabaseRepositoryService _dbRepo;

        public RecallController(
            ILogger<RecallController> logger,
            IServiceScopeFactory scopeFactory,
            ServerSessionService serverSessionService,
            DatabaseRepositoryService dbRepo
        )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _serverSessionService = serverSessionService;
            _dbRepo = dbRepo;
        }

       
    }
}
