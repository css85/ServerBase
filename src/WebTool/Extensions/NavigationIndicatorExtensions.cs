using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebTool.Extensions
{
    public static class NavigationIndicatorExtensions
    {
        private static readonly string _indexString = "/Index";
        private static readonly char[] _indexCharArray = _indexString.ToCharArray();

        public static string MakeDashboardMenuActive(this IHtmlHelper htmlHelper, string menuUrl)
        {
            var viewContext = htmlHelper.ViewContext;
            var currentPageUrl = viewContext.ViewData["ActiveMenu"] as string ?? viewContext.HttpContext.Request.Path;
            return currentPageUrl == menuUrl || currentPageUrl == _indexString ? "active" : null;
        }

        public static string MakeMenuActive(this IHtmlHelper htmlHelper, string menuUrl)
        {
            return htmlHelper.IsMenuActive(menuUrl) ? "active" : null;
        }

        public static string MakeMenuOpen(this IHtmlHelper htmlHelper, string menuUrl)
        {
            return htmlHelper.IsMenuActive(menuUrl) ? "menu-open" : null;
        }

        public static bool IsMenuActive(this IHtmlHelper htmlHelper, string menuItemUrl)
        {
            var viewContext = htmlHelper.ViewContext;
            var currentPageUrl = viewContext.ViewData["ActiveMenu"] as string ?? viewContext.HttpContext.Request.Path;
            return currentPageUrl.StartsWith(menuItemUrl) || currentPageUrl.StartsWith(menuItemUrl.TrimEnd(_indexCharArray)+"/");
        }
    }
}