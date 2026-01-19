using System.Linq;
using System.Threading.Tasks;
using TwelveMoments.Shared.Common;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Shared.Models.Server;
using Shared.Server.Define;
using Shared.Services.Redis;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace WebTool.Pages.Server
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDatabaseAsync _internalRedis;

        public ServerInfo[] ServerInfos { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _internalRedis = redisRepo.GetDb(RedisDatabase.Internal);
        }

        public async Task OnGetAsync()
        {
            var serverInfos = await _internalRedis.HashGetAllAsync(RedisKeys.hs_ServerInfoMap);

            ServerInfos = serverInfos.Select(p => JsonTextSerializer.Deserialize<ServerInfo>(p.Value.ToString())).ToArray();
        }
    }
}
