using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebTool.Identity.Base;

namespace WebTool.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "이메일을 입력해주세요.")]
            [EmailAddress(ErrorMessage = "올바르지 않은 이메일 주소입니다.")]
            [Display(Name = "이메일", Prompt = "example@example.com")]
            public string Email { get; set; }

            [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
            [StringLength(32, ErrorMessage = "비밀번호는 {2}자 이상, {1}자 이하로 입력해주세요.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "비밀번호", Prompt = "비밀번호")]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, false);
                if (signInResult.Succeeded)
                {
                    _logger.LogInformation("User login.");
                    return RedirectToPage("LoginComplete", new {returnUrl});
                }

                ModelState.AddModelError(string.Empty, "이메일과 비밀번호가 일치하지 않습니다.");
                return Page();
            }

            return Page();
        }
    }
}
