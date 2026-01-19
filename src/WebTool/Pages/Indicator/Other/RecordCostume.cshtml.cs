using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTool.Pages.Indicator
{
    public class RecordCostumeModel : PageModel
    {
        public string[] buyHistoryColumn = { "Index", "FileName", "CostumeName", "CostumeShopType", "7일", "30일", "무기한" };
        public string[] hasCostumeColumn = { "Index", "FileName", "CostumeName", "CostumeShopType", "보유량" };
        public void OnGet()
        {
        }
    }
}
