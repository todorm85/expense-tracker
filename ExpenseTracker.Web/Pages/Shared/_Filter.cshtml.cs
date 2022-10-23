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
    public class FiltersViewModel
    {
        private const string UncategorisedOptionValue = "-";

        public FiltersViewModel()
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

        public void Init(IEnumerable<string> categories)
        {
            if (SelectedCategories == null)
            {
                SelectedCategories = categories?.Where(x => x != "ignored").ToList() ?? new List<string>();
                SelectedCategories.Add(UncategorisedOptionValue);
            }

            CategoriesDropDownModel = CategoriesDropDownModel.Union(categories.Select(x => new SelectListItem() { Text = x, Value = x })).ToList();
        }

        public Expression<Func<Transaction, bool>> GetFilterQuery()
        {
            return x => ApplyDateFilter(x) &&
                                ApplyCategoriesFilter(x) &&
                                ApplySearchFilter(x) &&
                                !x.Ignored;
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
    }
}
