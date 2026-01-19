using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Shared.CsvData;
using Shared.ServerApp.Services;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Server.Define;
using WebTool.Identity;
using WebTool.Base.UserInfoDetail;

namespace WebTool.Pages.User.UserWindowPopup
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class DetailCostumeInfoModel : PageModel
    {
        public long UserSeq;

//        public CurrentAvatar avatar;
        public TableInfoItem[] InfoDatas;
        public DetailCostumeInfoModel()
        {
        }

        public void OnGetAsync(long userSeq)
        {
            UserSeq = userSeq;
        }
    }
}
