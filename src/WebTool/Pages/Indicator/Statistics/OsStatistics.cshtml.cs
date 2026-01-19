using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator
{
    public class OsStatisticsModel : PageModel
    {
        public string[] columnHeader = new string[] { "시간", "OS", "UV", "NUV", "PCCU", "ACCU", "로그인 횟수" };
        public void OnGet()
        {
        }
    }
}
