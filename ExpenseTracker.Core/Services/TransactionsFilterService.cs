using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Web.Pages.Shared;

namespace ExpenseTracker.Core.Services;

public class TransactionsFilterService : ItemsFilterService<Transaction, TransactionsFilterResult, TransactionsFilterParams>
{

    private const string UnknownSourceLabel = "unknown";

    private TransactionsFilterParams _filterParams = new TransactionsFilterParams();

    private CategoryExpression _parsedCategoryExpression;

    public TransactionsFilterService(IReadRepository<Transaction> repository) : base(repository)
    {
    }

    public TransactionsFilterResult GetFilteredTransactions(TransactionsFilterParams filterParams = null)
    {
        // TODO: cache categories and sources if slow, but unlikely for use cases
        this._filterParams = filterParams ?? new TransactionsFilterParams();
        var byDateBySearch = repository.GetAll(GetFilterQuery(FilterBy.Date | FilterBy.Search));
        var filteredBySource = byDateBySearch.AsQueryable().Where(GetFilterQuery(FilterBy.Source)).ToList();
        var latestSources = byDateBySearch.Select(x => x.Source).OrderBy(x => x).Distinct().ToList();

        List<string> latestCategories = filteredBySource
            .SelectMany(x => string.IsNullOrWhiteSpace(x.Category) ? [string.Empty] : x.Category.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .OrderBy(x => x)
            .Distinct()
            .ToList();

        SetUncategorizedValue(latestCategories);

        filterParams.Filter = GetFilterQuery();
        var result = base.GetFilteredItems(filterParams);
        result.AvailableCategories = latestCategories;
        result.AvailableSources = latestSources;

        return result;
    }

    private static void SetUncategorizedValue(List<string> latestCategories)
    {
        var uncategorizedIndex = latestCategories.IndexOf(string.Empty);
        if (uncategorizedIndex < 0)
            uncategorizedIndex = latestCategories.IndexOf(null!);
        if (uncategorizedIndex > -1)
            latestCategories[uncategorizedIndex] = TransactionsFilterParams.UncategorizedOptionValue;
    }

    private Expression<Func<Transaction, bool>> GetFilterQuery(FilterBy flags = FilterBy.Date | FilterBy.Category | FilterBy.Search | FilterBy.Source)
    {
        return x => ApplyDateFilter(x, flags) &&
                ApplyCategoriesFilter(x, flags) &&
                ApplySearchFilter(x, flags) &&
                ApplySourceFilter(x, flags);
    }

    private bool ApplySourceFilter(Transaction x, FilterBy flags)
    {
        if (!flags.HasFlag(FilterBy.Source) || _filterParams.SelectedSources.Count == 0)
            return true;

        return _filterParams.SelectedSources.Contains(x.Source);
    }

    private bool ApplyCategoriesFilter(Transaction x, FilterBy flags)
    {
        if (!flags.HasFlag(FilterBy.Category))
            return true;
        
         var itemCategories = x.Category?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        // First, check if we have a category expression to evaluate
        if (!string.IsNullOrWhiteSpace(_filterParams.CategoryExpression))
        {
            if (_parsedCategoryExpression == null)
                _parsedCategoryExpression = new CategoryExpression(_filterParams.CategoryExpression);

            return _parsedCategoryExpression.Evaluate(itemCategories);
        }

        // Fall back to the traditional category selection logic
        if (_filterParams.SelectedCategories.Count > 0)
        {
            if (_filterParams.SelectedCategories.Contains(TransactionsFilterParams.UncategorizedOptionValue) && string.IsNullOrWhiteSpace(x.Category))
                return true;

            if (_filterParams.SelectedCategories.Any(itemCategories.Contains))
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
        var startDate = _filterParams.DateFrom.ToDayStart();
        var endDate = _filterParams.DateTo.AddDays(1).ToDayStart();
        return x.Date >= startDate && x.Date < endDate;
    }

    private bool ApplySearchFilter(Transaction x, FilterBy flags)
    {
        if (!flags.HasFlag(FilterBy.Search))
            return true;
        if (!string.IsNullOrWhiteSpace(_filterParams.Search) && x != null && x.Details != null)
        {
            return x.Details.ToLowerInvariant().Contains(_filterParams.Search.ToLowerInvariant());
        }

        return true;
    }

    
}
