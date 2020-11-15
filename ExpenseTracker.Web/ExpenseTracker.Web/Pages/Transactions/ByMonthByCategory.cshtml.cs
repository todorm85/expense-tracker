using ExpenseTracker.Core;
using ExpenseTracker.Web.Models.Transactions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class TransactionsByMonthByCategoryModel : GridBase
    {
        public TransactionsByMonthByCategoryModel(
            ITransactionsService transactionsService, CategoriesService categoriesService) : base(transactionsService, categoriesService)
        {
            this.pageName = "ByMonthByCategory";
            this.initialMonthsBack = -5;
        }

        public IDictionary<DateTime, IDictionary<string, decimal>> MonthsCategoriesTotals { get; set; }

        public IDictionary<DateTime, decimal> MonthsTotals { get; private set; }
        public IDictionary<DateTime, decimal> MonthsIncomeTotals { get; private set; }

        public IDictionary<DateTime, bool> ExpandedMonths { get; set; }
        public IDictionary<DateTime, IDictionary<string, bool>> ExpandedCategories { get; set; }
        public decimal AverageExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal AverageBalance { get; private set; }
        public Dictionary<string, decimal[]> CategoriesAverages { get; private set; }

        protected override void InitializeTransactions()
        {
            InitializeExpanded();
            GetTransactions();
        }

        protected override void InitializeFilters()
        {
            if (!this.HttpContext.Request.Query.TryGetValue("SortBy", out StringValues result))
            {
                this.Filters.SortBy = SortOptions.Amount;
            }

            base.InitializeFilters();
        }

        protected override RouteValueDictionary GetQueryParameters()
        {
            var res = base.GetQueryParameters();
            res.Add("expanded", this.Request.Query["expanded"]);
            return res;
        }

        private void GetTransactions()
        {
            this.MonthsCategoriesTotals = new Dictionary<DateTime, IDictionary<string, decimal>>();
            this.MonthsTotals = new Dictionary<DateTime, decimal>();
            this.MonthsIncomeTotals = new Dictionary<DateTime, decimal>();
            this.Transactions = new List<Transaction>();
            this.CategoriesAverages = new Dictionary<string, decimal[]>();

            var all = this.GetTransactionsFiltered();
            if (all.Count() == 0)
            {
                return;
            }

            var expenses = all.Where(x => x.Type == TransactionType.Expense);
            var income = all.Where(x => x.Type == TransactionType.Income);
            var months = expenses
                .ToLookup(x => x.Date.SetToBeginningOfMonth()).OrderByDescending(x => x.Key);
            var monthsIncome = income
                .ToLookup(x => x.Date.SetToBeginningOfMonth());
            foreach (var month in months)
            {
                this.MonthsTotals[month.Key] = month.Sum(x => x.Amount);
                this.MonthsIncomeTotals[month.Key] = monthsIncome[month.Key].Sum(x => x.Amount);
                this.MonthsCategoriesTotals[month.Key] = new Dictionary<string, decimal>();
                var categories = month.ToLookup(x => x.Category).OrderByDescending(x => x.Sum(y => y.Amount));
                foreach (var c in categories)
                {
                    this.MonthsCategoriesTotals[month.Key][c.Key ?? ""] = c.Sum(x => x.Amount);

                    var orderedCats = c.OrderByDescending(x => x.Amount);
                    if (this.Filters.SortBy == SortOptions.Date)
                    {
                        orderedCats = c.OrderByDescending(x => x.Date);
                    }

                    foreach (var t in orderedCats)
                    {
                        this.Transactions.Add(t);
                    }
                }
            }

            this.AverageExpense = this.MonthsTotals.Sum(x => x.Value) / this.MonthsTotals.Count;
            this.AverageIncome = this.MonthsIncomeTotals.Sum(x => x.Value) / this.MonthsTotals.Count;
            decimal allBalance = 0;
            foreach (var monthIncome in MonthsIncomeTotals)
            {
                allBalance += monthIncome.Value - MonthsTotals[monthIncome.Key];
            }

            this.AverageBalance = allBalance / this.MonthsTotals.Count;
            var categoriesAveragesTemp = new Dictionary<string, decimal[]>();
            foreach (var monthCatTotal in MonthsCategoriesTotals)
            {
                foreach (var cat in monthCatTotal.Value)
                {
                    if (this.CategoriesAverages.ContainsKey(cat.Key))
                    {
                        categoriesAveragesTemp[cat.Key][0] += cat.Value;
                        this.CategoriesAverages[cat.Key][0] += cat.Value;
                    }
                    else
                    {
                        categoriesAveragesTemp[cat.Key] = new decimal[] { cat.Value, 0m };
                        this.CategoriesAverages[cat.Key] = new decimal[] { cat.Value, 0m };
                    }
                }
            }

            foreach (var catAvg in categoriesAveragesTemp)
            {
                this.CategoriesAverages[catAvg.Key][1] = catAvg.Value[0] / this.MonthsTotals.Count;
            }
        }

        private void InitializeExpanded()
        {
            this.ExpandedMonths = new Dictionary<DateTime, bool>();
            this.ExpandedCategories = new Dictionary<DateTime, IDictionary<string, bool>>();
            var expandedQuery = this.Request.Query["expanded"].ToString();
            if (!string.IsNullOrEmpty(expandedQuery))
            {
                var expandedElements = expandedQuery.Split(",", StringSplitOptions.RemoveEmptyEntries);
                foreach (var expandedElement in expandedElements)
                {
                    var expandedElementParts = expandedElement.Split("__");
                    var type = expandedElementParts[0];
                    var date = DateTime.ParseExact(expandedElementParts[1], "MM_yy", CultureInfo.InvariantCulture).SetToBeginningOfMonth();
                    if (type == "category")
                    {
                        var category = expandedElementParts[2];
                        if (!ExpandedCategories.ContainsKey(date))
                        {
                            ExpandedCategories.Add(date, new Dictionary<string, bool>());
                        }

                        if (!ExpandedCategories[date].ContainsKey(category))
                        {
                            ExpandedCategories[date].Add(category, true);
                        }
                    }
                    else
                    {
                        if (!ExpandedMonths.ContainsKey(date))
                        {
                            ExpandedMonths.Add(date, true);
                        }
                    }
                }
            }
        }        
    }
}
