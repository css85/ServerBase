using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebTool.Identity;

namespace WebTool.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        private readonly ILogger<AccessDeniedModel> _logger;

        public string ReturnUrl;

        public AccessDeniedModel(
            ILogger<AccessDeniedModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(nameof(RoleType.User)) == false)
                    return RedirectToPage("RegisterConfirm", new { returnUrl });

                return Page();
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");

            return Page();
        }
    }
}