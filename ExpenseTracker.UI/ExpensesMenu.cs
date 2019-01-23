﻿using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.UI
{
    public class ExpensesMenu : DataItemMenuBase<Transaction>
    {
        private readonly ITransactionsService expenseService;

        private readonly IBudgetService budgetService;
        private readonly IBudgetCalculator budgetCalculator;

        public ExpensesMenu(ITransactionsService expensesService, IBudgetService budgetService, IOutputRenderer renderer, IBudgetCalculator budgetCalculator) : base(renderer)
        {
            this.Service = expensesService;
            this.expenseService = expensesService;
            this.budgetService = budgetService;
            this.budgetCalculator = budgetCalculator;
        }

        public override IBaseDataItemService<Transaction> Service { get; set; }

        [MenuAction("sec", "Show expenses (categories only)")]
        public void ShowExpensesCategoriesOnly()
        {
            this.ShowExpenses(false);
        }

        [MenuAction("sea", "Show expenses (all)")]
        public void ShowExpensesAll()
        {
            this.ShowExpenses(true);
        }

        [MenuAction("cl", "Classify all expenses")]
        public void Categorize()
        {
            this.expenseService.Classify();
        }

        [MenuAction("qa", "Quick add expense")]
        public void QuickAddExpense()
        {
            var amount = int.Parse(this.Renderer.PromptInput("Amount: ", "0"));
            var cat = this.Renderer.PromptInput("Category: ", string.Empty);
            var desc = this.Renderer.PromptInput("Description: ", string.Empty);
            var date = DateTime.Parse(this.Renderer.PromptInput("Date: ", DateTime.Now.ToString()));
            this.Service.Add(new Transaction[]
            {
                new Transaction()
                {
                    Amount = amount,
                    Category = cat,
                    Source = desc,
                    Date = date
                }
            });
        }

        private void ShowExpenses(bool detailed)
        {
            var fromDate = DateTime.Now.AddYears(-1);
            var toDate = DateTime.Now;
            this.Renderer.GetDateFilter(ref fromDate, ref toDate);

            var categoriesByMonth = this.expenseService.GetExpensesByCategoriesByMonths(fromDate, toDate);
            foreach (var month in categoriesByMonth.OrderBy(x => x.Key))
            {
                var monthBudget = this.budgetService.GetCumulativeForMonth(month.Key);

                this.Renderer.WriteLine();
                this.WriteMonthLabel(month, monthBudget);
                this.Renderer.WriteLine();

                foreach (var category in month.Value.OrderBy(x => x.Key))
                {
                    this.WriteMonthCategoryLabel(monthBudget, category, 5);
                    if (detailed)
                    {
                        foreach (var e in category.Value.OrderBy(x => x.Date))
                        {
                            this.WriteExpenseDetails(e, 10);
                        }
                    }
                }
            }
        }

        private void WriteExpenseDetails(Transaction e, int padding = 0)
        {
            var source = e.Source?.ToString() ?? "";
            if (source.Length > 43)
            {
                source = source.Substring(0, 40) + "...";
            }

            source = source.PadLeft(45);

            this.Renderer.WriteLine("".PadLeft(padding) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {source} {e.Amount.ToString("F0").PadLeft(10)} {e.Category?.ToString().PadLeft(10)}");
        }

        private void WriteMonthCategoryLabel(Budget monthBudget, KeyValuePair<string, IEnumerable<Transaction>> category, int pad = 0)
        {
            var categoryName = string.IsNullOrEmpty(category.Key) ? "unknown" : category.Key;
            var categoryActual = category.Value.Sum(e => e.Amount);
            var budgetCategoryExists = monthBudget?.ExpectedTransactions.Any(x => x.Category == category.Key && x.Type == TransactionType.Expense);
            var catExpected = budgetCategoryExists.HasValue && budgetCategoryExists.Value ?
                monthBudget?.ExpectedTransactions.Where(x => x.Category == category.Key && x.Type == TransactionType.Expense).Sum(x => x.Amount) : null;
            this.Renderer.Write("".PadLeft(pad) + $"{categoryName} : {categoryActual.ToString("F0")} ");
            if (catExpected != null)
            {
                this.WriteBudget(categoryActual, catExpected.Value);
            }

            this.Renderer.WriteLine();
        }

        private void WriteMonthLabel(KeyValuePair<DateTime, Dictionary<string, IEnumerable<Transaction>>> month, Budget monthBudget)
        {
            var monthActualTotal = month.Value.Sum(x => x.Value.Sum(y => y.Amount));
            this.Renderer.Write($"{month.Key.ToString("MMMM")}: {monthActualTotal.ToString("F0")} ");
            decimal? monthExpected = null;
            if (monthBudget != null)
            {
                monthExpected = this.budgetCalculator.CalculateExpectedExpenses(monthBudget);
            }

            if (month.Key >= DateTime.Now && monthExpected != null)
            {
                this.WriteBudget(monthActualTotal, monthExpected.Value);
                this.WriteBudgetSavings(monthActualTotal, monthExpected.Value, monthBudget);
            }

            this.Renderer.WriteLine();
        }

        private void WriteBudget(decimal actualExpenses, decimal expectedExpenses)
        {
            var diff = expectedExpenses - actualExpenses;
            this.Renderer.Write($"{expectedExpenses.ToString("F0")} ", Style.MoreInfo);
            var style = diff >= 0 ? Style.Success : Style.Error;
            this.Renderer.Write($"{diff.ToString("F0")}", style);
        }

        private void WriteBudgetSavings(decimal actualExpenses, decimal expectedExpenses, Budget monthBudget)
        {
            this.Renderer.Write($" (Savings:");
            var monthActual = this.budgetCalculator.CalculateActualIncome(monthBudget);

            var actualSavings = monthActual - actualExpenses;
            this.Renderer.Write($"{actualSavings}", actualSavings >= 0 ? Style.Success : Style.Error);

            this.Renderer.Write(")");
        }
    }
}