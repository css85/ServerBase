using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Identity;

namespace WebTool.Pages.User
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class ReportListModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
