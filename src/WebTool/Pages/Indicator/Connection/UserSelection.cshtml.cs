using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator.Connection
{
    public class UserSelectionModel : PageModel
    {
        public string[] columnHeader = new string[] { "날짜", "-Day", "ID", "닉네임" };
        public void OnGet()
        {
        }
    }
}
