using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator.Connection
{
    public class TotalConnectionModel : PageModel
    {
        public string[] columnHeader = new string[] { "³¯Â¥", "-Day", "UV" };

        public void OnGet()
        {
        }
    }
}
