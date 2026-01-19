using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.ServerApp.Services;
using WebTool.Identity;

namespace WebTool.Pages.Operation.Event
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeUser)]
    public class View : PageModel
    {
        public readonly CsvStoreContextData CsvData;


        public View(
            CsvStoreContext csvStoreContext
            )
        {
            CsvData = csvStoreContext.GetData();
        }

        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }
    }
}