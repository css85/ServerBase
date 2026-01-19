using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;

namespace WebTool.Pages.Operation.Recall
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class RecallItemModel : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}