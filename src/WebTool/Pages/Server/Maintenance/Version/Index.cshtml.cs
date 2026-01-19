using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;

namespace WebTool.Pages.Server.Config.Version
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
