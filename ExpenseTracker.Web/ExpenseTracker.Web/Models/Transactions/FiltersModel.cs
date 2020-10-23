using ExpenseTracker.Web.Pages.Transactions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Web.Models.Transactions
{
    public class FiltersModel
    {
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string CategoryFilter { get; set; }

        public SortOptions SortBy { get; set; }

        public string Search { get; set; }

        public List<SelectListItem> Categories { get; set; }
    }
}
