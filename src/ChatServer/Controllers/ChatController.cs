using System;
using ChatServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;

namespace GateServer.Controllers
{
    [Route("chat")]
    public class ChatController : BaseApiController
    {
        private readonly ILogger<ChatController> _logger;
        private readonly CsvStoreContext _csvContext;
        private readonly ChatCheckService _chatCheckService;

        public ChatController(
            ILogger<ChatController> logger,
            CsvStoreContext csvContext,
            ChatCheckService gateCheckService
            )
        {
            _logger = logger;
            _csvContext = csvContext;
            _chatCheckService = gateCheckService;   
        }

    }
}
