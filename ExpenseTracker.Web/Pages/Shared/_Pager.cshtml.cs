using System.Collections.Generic;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class PagerModel
    {
        public IDictionary<string,string> RouteParams { get; set; } = new Dictionary<string,string>();
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
