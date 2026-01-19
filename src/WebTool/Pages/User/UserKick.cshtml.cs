using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Shared.Packet.Models;
using Shared.Services.Redis;
using Shared.Server.Define;
using Shared.Packet;

namespace WebTool.Pages.User
{
    public class UserKickModel : PageModel
    {
        private readonly ILogger<UserKickModel> _logger;
        private readonly IDatabaseAsync _channelRedis;

        public Dictionary<int, int> ChannelAppId;

        public UserKickModel(
            ILogger<UserKickModel> logger,
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            
        }
    }
}
