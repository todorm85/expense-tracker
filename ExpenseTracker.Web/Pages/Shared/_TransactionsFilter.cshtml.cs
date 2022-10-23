using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Web.Pages.Transactions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class TransactionsFilterViewModel
    {
        private const string UncategorisedOptionValue = "-";

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

            CategoriesDropDownModel = new List<SelectListItem>()
            {
                new SelectListItem("uncategorised", UncategorisedOptionValue)
            };

            SelectedCategories = new List<string>();
        }

        [JsonIgnore]
        public List<SelectListItem> CategoriesDropDownModel { get; set; }
        public List<string> SelectedCategories { get; set; }
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
        public string Search { get; set; }
        public SortOptions SortBy { get; set; }
        [JsonIgnore]
        public bool HideSorting { get; set; }

        /// <summary>
        /// Using business logic in model for partial as data binding is not working for view components!
        /// </summary>
        /// <param name="service"></param>
        public void Init(ITransactionsService service)
        {
            var allTransactions = service.GetAll(GetFilterQuery(FilterBy.Date | FilterBy.Search));
            IEnumerable<string> categories = allTransactions.Where(x => !string.IsNullOrEmpty(x.Category))
                                   .Select(x => x.Category)
                                   .OrderBy(x => x)
                                   .Distinct();

            if (SelectedCategories == null)
            {
                SelectedCategories = categories?.Where(x => x != "ignored").ToList() ?? new List<string>();
                SelectedCategories.Add(UncategorisedOptionValue);
            }
            else
            {
                SelectedCategories = SelectedCategories.Where(x => categories.Contains(x)).ToList();
            }

            CategoriesDropDownModel = CategoriesDropDownModel.Union(categories.Select(x => new SelectListItem() { Text = x, Value = x })).ToList();
        }

        public Expression<Func<Transaction, bool>> GetFilterQuery(FilterBy flags = FilterBy.Date | FilterBy.Category | FilterBy.Search)
        {
            return x => ApplyDateFilter(x, flags) &&
                    ApplyCategoriesFilter(x, flags) &&
                    ApplySearchFilter(x, flags);
        }
        private bool ApplyCategoriesFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Category))
                return true;
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

        internal static TransactionsFilterViewModel FromString(string filter, ITransactionsService transactionsService)
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
    }
}
