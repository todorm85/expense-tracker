using System.Collections.Generic;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class PagerModel
    {
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
