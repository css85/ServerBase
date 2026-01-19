using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebTool.Identity;
using WebTool.Identity.Base;

namespace WebTool.Areas.Identity.Pages.Account
{
    public class LoginCompleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginCompleteModel(
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");


            //Update
            foreach (RoleType item in System.Enum.GetValues(typeof(RoleType)))
            {
                bool value = User.IsInRole(item.ToString());
                if (value)
                {
                    return Redirect(returnUrl = @"~/Server");
                }
            }

            return RedirectToPage("RegisterConfirm", new { returnUrl });

            //Before
            //if (User.IsInRole(nameof(RoleType.User)) == false)
            //    return RedirectToPage("RegisterConfirm", new { returnUrl });

            //return LocalRedirect(returnUrl);
        }
    }
}
