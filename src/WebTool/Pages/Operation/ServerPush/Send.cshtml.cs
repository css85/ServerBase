using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Repository;
using WebTool.Database;
using WebTool.Identity;

namespace WebTool.Pages.Operation.ServerPush
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class SendModel : PageModel
    {
        public class SimplePreset
        {
            public long Seq;
            public string Name;
        }

        private readonly ILogger<SendModel> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public List<SimplePreset> Presets { get; set; }

        public SendModel(
            ILogger<SendModel> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

       
    }
}
