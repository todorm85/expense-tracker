using Microsoft.AspNetCore.Mvc.Rendering;
using ExpenseTracker.Core.Services.Models;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class TransactionsFilterViewModel
    {
        private DateTime _dateFrom = DateTime.UtcNow.ToMonthStart();
        private DateTime _dateTo = DateTime.UtcNow;

        public DateTime DateFrom
        {
            get => _dateFrom;
            set => _dateFrom = (value == default) ? DateTime.UtcNow.ToMonthStart() : value;
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set => _dateTo = (value == default) ? DateTime.UtcNow : value;
        }

        public bool HideSorting { get; set; }
        public string Search { get; set; } = string.Empty;
        public SortOptions SortBy { get; set; }
        public bool Asc { get; set; }
        public string CategoryExpression { get; set; } = string.Empty;
        public List<string> SelectedCategories { get; set; } = new List<string>();
        public List<SelectListItem> CategoriesDropDownModel { get; set; } = new List<SelectListItem>();
        public List<string> SelectedSources { get; set; } = new List<string>();
        public List<SelectListItem> Sources { get; set; } = new List<SelectListItem>();
    }

    public static class TransactionsFilterViewModelExtensions
    {
        public static TransactionsFilterParams ToFilterParams(this TransactionsFilterViewModel vm, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var filterParams = new TransactionsFilterParams
            {
                DateFrom = vm.DateFrom,
                DateTo = vm.DateTo,
                Search = vm.Search,
                CategoryExpression = vm.CategoryExpression,
                SortOptions = vm.SortBy,
                Descending = !vm.Asc,
                SelectedSources = vm.SelectedSources,
                SelectedCategories = vm.SelectedCategories,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return filterParams;
        }
        
        public static void Apply(this TransactionsFilterViewModel vm, TransactionsFilterResult filterRes)
        {
            var selectedCategories = vm.SelectedCategories;
            var availableCategories = filterRes.AvailableCategories;

            List<SelectListItem> categoriesListItems = availableCategories.Select(x => new SelectListItem() { Text = x == TransactionsFilterParams.UncategorizedOptionValue ? "Uncategorized" : x, Value = x }).ToList();

            categoriesListItems.ForEach(x =>
            {
                if (selectedCategories.Count == 0 || selectedCategories.Contains(x.Value))
                {
                    x.Selected = true;
                }
            });

            vm.CategoriesDropDownModel = categoriesListItems;

            vm.Sources = filterRes.AvailableSources.Select(x => new SelectListItem() { Text = string.IsNullOrWhiteSpace(x) ? "Unknown" : x, Value = x }).ToList();
        }
    }
}
