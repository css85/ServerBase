using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;
namespace WebTool.Pages.Server.Config.HotTime
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class Index : PageModel
    {
        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }
    }
}