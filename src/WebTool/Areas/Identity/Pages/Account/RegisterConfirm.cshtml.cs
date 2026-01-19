using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebTool.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmModel : PageModel
    {
        private readonly ILogger<RegisterModel> _logger;

        public string ReturnUrl { get; set; }

        public RegisterConfirmModel(
            ILogger<RegisterModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ReturnUrl = returnUrl;
        }
    }
}