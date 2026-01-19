using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebTool.Extensions;
using WebTool.Identity;
using WebTool.Identity.Base;

namespace WebTool.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        public RegisterModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RegisterModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "이메일을 입력해주세요.")]
            [EmailAddress(ErrorMessage = "올바르지 않은 이메일 주소입니다.")]
            [Display(Name = "이메일", Prompt = "example@example.com")]
            public string Email { get; set; }

            [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
            [StringLength(32, ErrorMessage = "비밀번호는 {2}자 이상, {1}자 이하로 입력해주세요.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "비밀번호", Prompt = "6자 이상 32자 이하")]
            public string Password { get; set; }

            [Required(ErrorMessage = "비밀번호를 한번 더 입력해주세요.")]
            [DataType(DataType.Password)]
            [Display(Name = "비밀번호 확인", Prompt = "비밀번호 확인")]
            [Compare("Password", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
            public string ConfirmPassword { get; set; }
        }

        public Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    LockoutEnabled = false,
                };
                var createUserResult = await _userManager.CreateAsync(user, Input.Password);
                if (createUserResult.Succeeded)
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(user, nameof(RoleType.User));
                    if (addRoleResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToPage("LoginComplete", new {returnUrl});
                    }

                    ModelState.AddModelError(addRoleResult.Errors);
                }

                ModelState.AddModelError(createUserResult.Errors);
            }

            return Page();
        }
    }
}