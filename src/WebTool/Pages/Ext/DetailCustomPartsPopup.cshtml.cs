using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTool.Base.UserInfoDetail;

namespace WebTool.Pages.Ext
{
    public class DetailCustomPartsPopupModel : PageModel
    {
        public long UserSeq;

        public DetailCustomPartsPopupModel()
        {
        }

        public void OnGetAsync(long userSeq)
        {
            UserSeq = userSeq;
        }
    }
}
