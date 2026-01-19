using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared;
using WebTool.Identity;

namespace WebTool.Pages.Operation.Command
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class SendModel : PageModel
    {
        [BindProperty]
        public NetServiceType NetServiceType { get; set; } = NetServiceType.Gate;
        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }
    }
}