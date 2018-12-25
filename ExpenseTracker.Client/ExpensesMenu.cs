using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExpensesMenu : DataItemMenuBase<Expense>
    {
        public ExpensesMenu()
        {
            this.Service = ServicesFactory.GetService<ExpensesService>();
            this.expenseService = (ExpensesService)this.Service;
            this.budgetService = ServicesFactory.GetService<BudgetService>();
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

        private static void WriteExpenseDetails(Expense e, int padding = 0)
        {
            var source = e.Source?.ToString() ?? "";
            if (source.Length > 43)
            {
                source = source.Substring(0, 40) + "...";
            }

            source = source.PadLeft(45);

            Console.WriteLine("".PadLeft(padding) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {source} {e.Amount.ToString("00.00").PadLeft(10)} {e.Category?.ToString().PadLeft(10)}");
        }

        private static void WriteMonthCategoryLabel(Budget monthBudget, KeyValuePair<string, IEnumerable<Expense>> category, int pad = 0)
        {
            var categoryName = string.IsNullOrEmpty(category.Key) ? "unknown" : category.Key;
            var categoryActual = category.Value.Sum(e => e.Amount);
            var budgetCategoryExists = monthBudget?.ExpectedExpensesByCategory.ContainsKey(category.Key);
            var catExpected = budgetCategoryExists.HasValue && budgetCategoryExists.Value ? monthBudget?.ExpectedExpensesByCategory[category.Key] : null;
            var diff = (catExpected ?? 0) - categoryActual;
            Console.WriteLine("".PadLeft(pad) + $"{categoryName} : {categoryActual} {catExpected.ToString() ?? ""} {((catExpected != null) ? diff.ToString() : "")}");
        }

        private void WriteMonthLabel(KeyValuePair<DateTime, Dictionary<string, IEnumerable<Expense>>> month, Budget monthBudget)
        {
            var monthActualTotal = month.Value.Sum(x => x.Value.Sum(y => y.Amount));
            var monthExpected = monthBudget?.ExpectedExpensesByCategory.Sum(x => x.Value);
            var diff = (monthExpected ?? 0) - monthActualTotal;
            Console.Write($"{month.Key.ToString("MMMM")}: {monthActualTotal} ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{monthExpected?.ToString() ?? ""} ");
            Console.ForegroundColor = diff > 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"{((monthExpected == null) ? "" : diff.ToString())}");
            Console.ResetColor();
            Console.WriteLine();
        }

        public override BaseDataItemService<Expense> Service { get; set; }

        private ExpensesService expenseService;
        private BudgetService budgetService;
    }
}