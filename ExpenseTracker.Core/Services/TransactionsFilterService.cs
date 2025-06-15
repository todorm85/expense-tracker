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

    public TransactionsFilterService(IReadRepository<Transaction> repository) : base(repository)
    {
    }

    public TransactionsFilterResult GetFilteredTransactions(TransactionsFilterParams filterParams = null)
    {
        // TODO: cache categories and sources if slow, but unlikely for use cases
        this._filterParams = filterParams ?? new TransactionsFilterParams();
        filterParams.Build(FilterBy.Date | FilterBy.Search);
        var byDateBySearch = repository.GetAll(filterParams.Filter);
        var latestSources = byDateBySearch.Select(x => x.Source).OrderBy(x => x).Distinct().ToList();
        filterParams.Build(FilterBy.Source);
        var filteredBySource = byDateBySearch.AsQueryable().Where(filterParams.Filter).ToList();

        List<string> latestCategories = filteredBySource
            .SelectMany(x => string.IsNullOrWhiteSpace(x.Category) ? [string.Empty] : x.Category.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .OrderBy(x => x)
            .Distinct()
            .ToList();

        SetUncategorizedValue(latestCategories);

        filterParams.Build();
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

    

    
}
