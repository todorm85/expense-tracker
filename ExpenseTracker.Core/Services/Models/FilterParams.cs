using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Web.Pages.Shared;

namespace ExpenseTracker.Core.Services.Models
{
    public class TransactionsFilterParams : ItemsFilterParams<Transaction>
    {

        public const string UncategorizedOptionValue = "-";

        private CategoryExpression _parsedCategoryExpression;

        public TransactionsFilterParams()
        {
            SelectedCategories = new List<string>();
            CategoryExpression = string.Empty;
            Search = string.Empty;
        }

        public List<string> SelectedCategories { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string Search { get; set; }
        public List<string> SelectedSources { get; set; } = new List<string>() { UncategorizedOptionValue };
        public string CategoryExpression { get; set; }
        public SortOptions SortOptions { get; set; }

        public void Build(FilterBy filterFlags = FilterBy.Date | FilterBy.Category | FilterBy.Search | FilterBy.Source)
        {
            this.BuildFilterQuery(filterFlags);
            this.BuildSortQuery();
        }

        private void BuildSortQuery()
        {
            switch (SortOptions)
            {
                case SortOptions.Date:
                    this.OrderBy = x => x.Date;
                    break;
                case SortOptions.Amount:
                    this.OrderBy = x => x.Amount;
                    break;
                case SortOptions.Category:
                    this.OrderBy = x => x.Category ?? string.Empty;
                    break;
                case SortOptions.None:
                default:
                    this.OrderBy = null;
                    break;
            }
        }

        private void BuildFilterQuery(FilterBy flags)
        {
            this.Filter = x => ApplyDateFilter(x, flags) &&
                    ApplyCategoriesFilter(x, flags) &&
                    ApplySearchFilter(x, flags) &&
                    ApplySourceFilter(x, flags);
        }

        private bool ApplySourceFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Source) || this.SelectedSources.Count == 0)
                return true;

            return this.SelectedSources.Contains(x.Source);
        }

        private bool ApplyCategoriesFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Category))
                return true;

            var itemCategories = x.Category?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            // First, check if we have a category expression to evaluate
            if (!string.IsNullOrWhiteSpace(this.CategoryExpression))
            {
                if (_parsedCategoryExpression == null)
                    _parsedCategoryExpression = new CategoryExpression(this.CategoryExpression);

                return _parsedCategoryExpression.Evaluate(itemCategories);
            }

            // Fall back to the traditional category selection logic
            if (this.SelectedCategories.Count > 0)
            {
                if (this.SelectedCategories.Contains(TransactionsFilterParams.UncategorizedOptionValue) && string.IsNullOrWhiteSpace(x.Category))
                    return true;

                if (this.SelectedCategories.Any(itemCategories.Contains))
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
            var startDate = this.DateFrom.ToDayStart();
            var endDate = this.DateTo.AddDays(1).ToDayStart();
            return x.Date >= startDate && x.Date < endDate;
        }

        private bool ApplySearchFilter(Transaction x, FilterBy flags)
        {
            if (!flags.HasFlag(FilterBy.Search))
                return true;
            if (!string.IsNullOrWhiteSpace(this.Search) && x != null && x.Details != null)
            {
                return x.Details.ToLowerInvariant().Contains(this.Search.ToLowerInvariant());
            }

            return true;
        }
    }
}