using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator.Users
{
    public class PopulationModel : PageModel
    {
        public string[] columnHeader = new string[] { "댄서 등급", "인원", "소비 루비", "소비 덴", "소비 컨디션", "결제 금액" };
        public void OnGet()
        {
        }
    }
}
