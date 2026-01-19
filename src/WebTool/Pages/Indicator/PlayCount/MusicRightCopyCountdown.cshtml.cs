using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator.PlayCount
{
    public class MusicRightCopyCountdownModel : PageModel
    {
        public string[] columnHeader = new[]
        {
            "Index", "MusicName", "ArtistName", "BPM", "Time", "비선택", "선택", "카운트"
        };

        public void OnGet()
        {
        }
    }
}
