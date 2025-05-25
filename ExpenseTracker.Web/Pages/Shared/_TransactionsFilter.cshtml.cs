using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Web.Pages.Transactions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class TransactionsFilterViewModel
    {
        private const string UncategorisedOptionValue = "-";
        private const string UnknownSourceLabel = "unknown";

        public TransactionsFilterViewModel()
        {
            if (DateTo == default)
            {
                DateTo = DateTime.UtcNow;
            }

            if (DateFrom == default)
            {
                DateFrom = DateTime.UtcNow.ToMonthStart();
            }

            SelectedCategories = new List<string>();
            AvailableCategories = new List<string>();
            CategoryExpression = string.Empty;
        }

        [JsonIgnore]
        public List<SelectListItem> CategoriesDropDownModel { get; set; }
        public List<string> SelectedCategories { get; set; }
        [JsonIgnore]
        public List<SelectListItem> AvailableCategoriesDropDownModel { get; set; }
        public List<string> AvailableCategories { get; set; }
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
        public string Search { get; set; }
        public SortOptions SortBy { get; set; }
        [JsonIgnore]
        public bool HideSorting { get; set; }
        public string Source { get; set; } = UncategorisedOptionValue;
        [JsonIgnore]
        public List<SelectListItem> Sources { get; set; } = new List<SelectListItem>() { new SelectListItem("all", UncategorisedOptionValue) };
        
        // New property for category expression
        public string CategoryExpression { get; set; }
        [JsonIgnore]
        private CategoryExpression? _parsedCategoryExpression;

        /// <summary>
        /// Using business logic in model for partial as data binding is not working for view components!
        /// </summary>
        /// <param name="service"></param>
        public void Init(IExpensesService service)
        {
            var allTransactions = service.GetAll(GetFilterQuery(FilterBy.Date | FilterBy.Search | FilterBy.Source));
            List<string> latestCategories = allTransactions
                .SelectMany(x => string.IsNullOrEmpty(x.Category) ? new string[1] { string.Empty } : x.Category.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .OrderBy(x => x)
                .Distinct()
                .ToList();

            SetUncategorizedValue(latestCategories);

            List<string> newlyAvailableCatesgories = new List<string>();
            if (AvailableCategories != null)
                newlyAvailableCatesgories = latestCategories.Where(x => !AvailableCategories.Contains(x)).ToList();

            AvailableCategories = latestCategories;


            if (SelectedCategories.Count > 0)
            {
                SelectedCategories = SelectedCategories.Where(x => latestCategories.Contains(x)).ToList();
            }
            else
            {
                SelectedCategories = latestCategories.Where(x => x != Constants.IgnoredCategory).ToList();
            }

            

            List<SelectListItem> selectListItems = latestCategories.Select(x => new SelectListItem() { Text = x == UncategorisedOptionValue ? "uncategorized" : x, Value = x }).ToList();
            CategoriesDropDownModel = selectListItems;
            AvailableCategoriesDropDownModel = selectListItems;

            var sources = service.GetAll(GetFilterQuery(FilterBy.Date | FilterBy.Search)).Select(x => x.Source).OrderBy(x => x).Distinct();
            Sources = Sources.Union(sources.Select(x => new SelectListItem() { Text = x ?? UnknownSourceLabel, Value = x })).ToList();
            
            // Initialize the category expression parser
            if (!string.IsNullOrWhiteSpace(CategoryExpression))
            {
                _parsedCategoryExpression = new CategoryExpression(CategoryExpression);
            }
        }

        private static void SetUncategorizedValue(List<string> latestCategories)
        {
            var uncategorizedIndex = latestCategories.IndexOf(string.Empty);
            if (uncategorizedIndex < 0)
                uncategorizedIndex = latestCategories.IndexOf(null);
            if (uncategorizedIndex > -1)
                latestCategories[uncategorizedIndex] = UncategorisedOptionValue;
        }

        public Expression<Func<Transaction, bool>> GetFilterQuery(FilterBy flags = FilterBy.Date | FilterBy.Category | FilterBy.Search | FilterBy.Source)
        {
            return x => ApplyDateFilter(x, flags) &&
                    ApplyCategoriesFilter(x, flags) &&
                    ApplySearchFilter(x, flags) &&
                    ApplySourceFilter(x, flags);
        }

        private bool ApplySourceFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Source) || Source == UncategorisedOptionValue)
                return true;

            return x.Source == Source || (Source == UnknownSourceLabel && string.IsNullOrEmpty(x.Source));
        }        private bool ApplyCategoriesFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Category))
                return true;
                
            // First, check if we have a category expression to evaluate
            if (!string.IsNullOrWhiteSpace(CategoryExpression))
            {
                if (_parsedCategoryExpression == null)
                    _parsedCategoryExpression = new CategoryExpression(CategoryExpression);
                
                var itemCategories = x.Category?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                return _parsedCategoryExpression.Evaluate(itemCategories);
            }
            
            // Fall back to the traditional category selection logic
            if (SelectedCategories.Count > 0)
            {
                if (SelectedCategories.Contains(UncategorisedOptionValue) && string.IsNullOrWhiteSpace(x.Category))
                    return true;

                var itemCategories = x.Category?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (itemCategories != null && SelectedCategories.Any(itemCategories.Contains))
                    return true;

                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ApplyDateFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Date))
                return true;
            return x.Date >= DateFrom.ToDayStart() && x.Date < DateTo.AddDays(1).ToDayStart();
        }

        private bool ApplySearchFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Search))
                return true;
            if (!string.IsNullOrWhiteSpace(Search) && x != null && x.Details != null)
            {
                return x.Details.ToLowerInvariant().Contains(Search.ToLowerInvariant());
            }

            return true;
        }

        public override string ToString()
        {
            return ModelSerialization.Serialize(this);
        }

        internal static TransactionsFilterViewModel FromString(string filter, IExpensesService transactionsService)
        {
            var result = ModelSerialization.Deserialize<TransactionsFilterViewModel>(filter) ?? new TransactionsFilterViewModel();
            result.Init(transactionsService);
            return result;
        }
    }

    [Flags]
    public enum FilterBy
    {
        None = 1,
        Date = 2,
        Category = 4,
        Search = 8,
        Source = 16,
    }
}
