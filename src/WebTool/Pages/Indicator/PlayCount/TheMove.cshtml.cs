using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator.PlayCount
{
    public class TheMoveModel : PageModel
    {
        public string[] columnHeader = new[]
        {
            "모드", "키", "난이도", "카운트"
        };
        public void OnGet()
        {
        }
    }
}
