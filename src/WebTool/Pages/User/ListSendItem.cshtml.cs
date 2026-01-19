using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;

namespace WebTool.Pages.User
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class ListSendItemModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}