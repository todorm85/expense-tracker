using System.Collections.Generic;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class PagerModel
    {
        public string PagePath { get; set; }
        public int CurrentPageIndex { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
