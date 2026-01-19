using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;
namespace WebTool.Pages.Operation.ServerPush
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeUser)]
    public class LogModel : PageModel
    {
        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }
    }
}
