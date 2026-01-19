using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.Server.Define;
using WebTool.Base.Item;
using WebTool.Services;

namespace WebTool.Pages.Ext
{
    public class ItemSelectPopupModel : PageModel
    {
        private readonly SelectItemService _selectItemService;

        public SelectItemType[] Categories { get; set; }
//        public PeriodType[] Periods { get; set; }

        public ItemSelectPopupModel(SelectItemService selectItemService)
        {
            _selectItemService = selectItemService;
        }

        public void OnGet()
        {
            Categories = _selectItemService.GetCategories();
  //          Periods = Enum.GetValues<PeriodType>().ToArray();
        }
    }
}
