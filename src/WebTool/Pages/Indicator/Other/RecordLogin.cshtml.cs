using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator
{
    public class RecordLoginModel : PageModel
    {
        public string[] columnHeader = new string[] {"ID", "닉네임", "로그인 시간", "로그아웃 시간"};
        public void OnGet()
        {
        }
    }
}
