using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.ServerApp.Services;
using WebTool.Identity;

namespace WebTool.Pages.Server.Config.Coupon
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeGm)]
    public class IndexModel : PageModel
    {
        private readonly CsvStoreContext _csvStoreContext;
        public IndexModel(CsvStoreContext csvStoreContext)
        {
            _csvStoreContext = csvStoreContext;
        }

        public long TotalSelectIndex()
        {
            return 0;
            //var couponReward = _csvStoreContext.GetData().CouponRewardListData;
            //return couponReward[couponReward.Count - 1].GroupId;
        }
    }
}
