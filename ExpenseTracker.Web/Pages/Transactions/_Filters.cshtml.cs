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
        private const string UncategorisedOptionValue = "-";
        private readonly ITransactionsService service;

        // for ModelBinding
        public FiltersModel() : this(null)
        { }

        public FiltersModel(ITransactionsService transactionsService)
        {
            if (DateTo == default)
            {
                this.DateTo = DateTime.UtcNow;
            }

            if (DateFrom == default)
            {
                this.DateFrom = DateTime.UtcNow.ToMonthStart();
            }

            this.Categories = new List<SelectListItem>()
            {
                new SelectListItem("uncategorised", UncategorisedOptionValue)
            };

            if (transactionsService != null)
            {
                // if not called by model binding preselect
                var allCats = transactionsService.GetAllCategories();
                this.SelectedCategories = allCats.Where(x => x != "ignored").ToList();
                this.SelectedCategories.Add(UncategorisedOptionValue);
            }
            else
            {
                // called by model binding
                this.SelectedCategories = new List<string>();
            }

            this.service = transactionsService;
        }

        public List<SelectListItem> Categories { get; set; }
        public List<string> SelectedCategories { get; set; }
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
            var allCats = transactionsService.GetAllCategories();

            this.Categories = this.Categories.Union(allCats.Select(x => new SelectListItem() { Text = x, Value = x })).ToList();
            return transactions;
        }

        private bool ApplyCategoriesFilter(Transaction x)
        {
            if (SelectedCategories.Count > 0)
            {
                foreach (var cat in SelectedCategories)
                {
                    if (cat == "-" && string.IsNullOrWhiteSpace(x.Category) ||
                        x.Category == cat)
                        return true;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ApplyDateFilter(Transaction x)
        {
            return x.Date >= DateFrom.ToDayStart() && x.Date < DateTo.AddDays(1).ToDayStart();
        }

        private bool ApplySearchFilter(Transaction x)
        {
            if (!string.IsNullOrWhiteSpace(Search) && x !=null && x.Details != null)
            {
                return x.Details.ToLowerInvariant().Contains(Search.ToLowerInvariant());
            }

            return true;
        }
    }
}