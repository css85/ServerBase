using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Ext
{
    public class UserBlockPopupModel : PageModel
    {
        public long UserSeq;
        public void OnGet(long userSeq)
        {
            UserSeq = userSeq;
        }
    }
}
