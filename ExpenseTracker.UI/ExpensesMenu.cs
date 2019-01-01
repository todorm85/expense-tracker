using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.UI
{
    public class ExpensesMenu : DataItemMenuBase<Expense>
    {
        public ExpensesMenu(IExpensesService expensesService, IBudgetService budgetService, IOutputRenderer renderer) : base(renderer)
        {
            this.Service = expensesService;
            this.expenseService = expensesService;
            this.budgetService = budgetService;
        }

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

        private void ShowExpenses(bool detailed)
        {
            var categoriesByMonth = this.expenseService.GetExpensesByCategoriesByMonths(DateTime.Now.AddYears(-1), DateTime.MaxValue);
            foreach (var month in categoriesByMonth.OrderBy(x => x.Key))
            {
                var monthBudget = this.budgetService.GetByMonth(month.Key);
                this.WriteMonthLabel(month, monthBudget);

                foreach (var category in month.Value.OrderBy(x => x.Key))
                {
                    WriteMonthCategoryLabel(monthBudget, category, 5);
                    if (detailed)
                    {
                        foreach (var e in category.Value.OrderBy(x => x.Date))
                        {
                            WriteExpenseDetails(e, 10);
                        }
                    }
                }
            }
        }

        private void WriteExpenseDetails(Expense e, int padding = 0)
        {
            var source = e.Source?.ToString() ?? "";
            if (source.Length > 43)
            {
                source = source.Substring(0, 40) + "...";
            }

            source = source.PadLeft(45);

            Renderer.WriteLine("".PadLeft(padding) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {source} {e.Amount.ToString("F0").PadLeft(10)} {e.Category?.ToString().PadLeft(10)}");
        }

        private void WriteMonthCategoryLabel(Budget monthBudget, KeyValuePair<string, IEnumerable<Expense>> category, int pad = 0)
        {
            var categoryName = string.IsNullOrEmpty(category.Key) ? "unknown" : category.Key;
            var categoryActual = category.Value.Sum(e => e.Amount);
            var budgetCategoryExists = monthBudget?.ExpectedExpensesByCategory.ContainsKey(category.Key);
            var catExpected = budgetCategoryExists.HasValue && budgetCategoryExists.Value ? monthBudget?.ExpectedExpensesByCategory[category.Key] : null;
            Renderer.Write("".PadLeft(pad) + $"{categoryName} : {categoryActual.ToString("F0")} ");
            if (catExpected != null)
            {
                WriteBudget(categoryActual, catExpected.Value);
            }

            Renderer.WriteLine();
        }

        private void WriteMonthLabel(KeyValuePair<DateTime, Dictionary<string, IEnumerable<Expense>>> month, Budget monthBudget)
        {
            var monthActualTotal = month.Value.Sum(x => x.Value.Sum(y => y.Amount));
            var monthExpected = monthBudget?.ExpectedExpensesByCategory.Sum(x => x.Value);
            Renderer.Write($"{month.Key.ToString("MMMM")}: {monthActualTotal.ToString("F0")} ");
            if (monthExpected != null)
            {
                WriteBudget(monthActualTotal, monthExpected.Value);
            }

            Renderer.WriteLine();
        }

        private void WriteBudget(decimal actual, decimal expected)
        {
            var diff = expected - actual;
            Renderer.Write($"{expected.ToString("F0")} ", Style.MoreInfo);
            var style = diff > 0 ? Style.Success : Style.Error;
            Renderer.Write($"{diff.ToString("F0")}", style);
        }

        public override IBaseDataItemService<Expense> Service { get; set; }

        private readonly IExpensesService expenseService;
        private readonly IBudgetService budgetService;
    }
}