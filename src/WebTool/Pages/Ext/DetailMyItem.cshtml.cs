using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.ServerApp.Services;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.CsvData;
using Shared.Entities.Models;
using Shared.Packet;
using Shared.CsvParser.Common;
using WebTool.Base.UserInfoDetail;

namespace WebTool.Pages.User.UserWindowPopup
{
    public class DetailMyItemModel : PageModel
    {
        public long UserSeq;

        public DetailMyItemProperty[] properties = new[]
        {
            new DetailMyItemProperty
            {
                TabName = "Total",
                TableRootId = "tab-content-totalItem",
                TableId = "table-totalItem",
                TableType = (int)DetailMyItemType.Total,
            },
            new DetailMyItemProperty
            {
                TabName = "Material",
                TableRootId = "tab-content-buffItem",
                TableId = "table-buffItem",
                TableType = (int)DetailMyItemType.Material,
            },
            new DetailMyItemProperty
            {
                TabName = "MarketingLeaflet",
                TableRootId = "tab-content-normalItem",
                TableId = "table-normalItem",
                TableType = (int)DetailMyItemType.MarketingLeaflet,
            },
            new DetailMyItemProperty
            {
                TabName = "SpecialBuff",
                TableRootId = "tab-content-couponItem",
                TableId = "table-couponItem",
                TableType = (int)DetailMyItemType.SpecialBuff,
            },
        };

        public void OnGet(long userSeq)
        {
            UserSeq = userSeq;
        }
    }
}