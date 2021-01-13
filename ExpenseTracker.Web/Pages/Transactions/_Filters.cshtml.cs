using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class FiltersModel
    {
        public FiltersModel() : this(0)
        {
        }

        public FiltersModel(int initialMonthsBack)
        {
            var now = DateTime.Now;
            if (DateTo == default)
            {
                this.DateTo = new DateTime(now.Year, now.Month, now.Day);
            }

            if (DateFrom == default)
            {
                this.DateFrom = DateTime.Now.AddMonths(-initialMonthsBack).ToMonthStart();
            }

            this.Categories = new List<SelectListItem>()
            {
                new SelectListItem("Select Category", ""),
                new SelectListItem("Uncategorised", "-")
            };
        }

        public List<SelectListItem> Categories { get; set; }
        public string CategoryFilter { get; set; }
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
        public string Search { get; set; }
        public SortOptions SortBy { get; set; }
        public bool HideSorting { get; set; }

        public IEnumerable<Transaction> GetTransactionsFiltered(ITransactionsService transactionsService)
        {
            var transactions = transactionsService
                            .GetAll(x => ApplyDateFilter(x) &&
                                ApplyCategoriesFilter(x) &&
                                ApplySearchFilter(x) &&
                                !x.Ignored);

            this.Categories = this.Categories.Union(
                transactions
                    .Where(x => !string.IsNullOrEmpty(x.Category))
                    .Select(x => x.Category)
                    .OrderBy(x => x)
                    .Distinct()
                    .Select(x => new SelectListItem() { Text = x, Value = x })).ToList();

            return transactions;
        }

        private bool ApplyCategoriesFilter(Transaction x)
        {
            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                if (CategoryFilter == "-")
                {
                    return string.IsNullOrWhiteSpace(x.Category);
                }
                else
                {
                    return x.Category == CategoryFilter;
                }
            }

            return true;
        }

        private bool ApplyDateFilter(Transaction x)
        {
            return x.Date >= DateFrom && x.Date <= DateTo.AddDays(1);
        }

        private bool ApplySearchFilter(Transaction x)
        {
            if (!string.IsNullOrWhiteSpace(Search))
            {
                return x.Details.Contains(Search);
            }

            return true;
        }
    }
}