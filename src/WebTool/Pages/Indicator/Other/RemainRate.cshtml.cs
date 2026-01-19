using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator
{
    public class RemainRateModel : PageModel
    {
        public string[] thDayRemain = new[]
        {
            "기준일", "NUV", "Day 1", "Day 2", "Day 3", "Day 4", "Day 5", "Day 6"
            , "Day 7", "Day 8", "Day 9", "Day 10", "Day 11", "Day 12", "Day 13", "Day 14", "Day 15"};

        public string[] thWeekRemain = new[]
        {
            "기준 기간", "NUV", "Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6"
            , "Week 7", "Week 8", "Week 9", "Week 10"
        };

        public void OnGet()
        {
        }
    }
}
