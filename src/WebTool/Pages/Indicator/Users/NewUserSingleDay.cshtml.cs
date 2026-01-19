using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator.Users
{
    public class NewUserSingleDayModel : PageModel
    {
        public string[] columnHeader = new string[] { "날짜", "ID", "닉네임", "유저 번호", "댄서 등급", "쇼핑몰 등급" };

        public void OnGet()
        {
        }
    }
}
