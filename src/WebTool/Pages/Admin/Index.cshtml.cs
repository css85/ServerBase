using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;

namespace WebTool.Pages.Admin
{
    [Authorize(Roles = nameof(RoleType.Admin))]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
