using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Repository;
using Shared.Repository.Services;
using WebTool.Database;
using WebTool.Identity;

namespace WebTool.Pages.Operation.MessageNotification
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
        private readonly DatabaseRepositoryService _dbRepo;

        public List<SimplePreset> Presets { get; set; }

        public SendModel(
            ILogger<SendModel> logger,
            DatabaseRepositoryService dbRepo
            )
        {
            _logger = logger;
            _dbRepo = dbRepo;
        }

        public async Task OnGetAsync()
        {
            

            //Presets = await appCtx.MessageNotificationPresets.Select(p => new SimplePreset
            //{
            //    Seq = p.Seq,
            //    Name = p.PresetName,
            //}).ToListAsync();
        }
    }
}
