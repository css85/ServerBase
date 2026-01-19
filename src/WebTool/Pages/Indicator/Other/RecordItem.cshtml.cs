using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator
{
    public class RecordItemModel : PageModel
    {
        public string[] buyTableTd = new string[] { "분류", "Index", "아이템 이름", "구매량" };
        public string[] useTableTd = new string[] { "분류", "Index", "아이템 이름", "사용량" };
        public void OnGet()
        {
        }
    }
}
