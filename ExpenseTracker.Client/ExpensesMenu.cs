using System;
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
                Console.WriteLine(month.Key.ToString("MMMM") + $": {month.Value.Sum(x => x.Value.Sum(y => y.Amount))}");
                foreach (var c in month.Value.OrderBy(x => x.Key))
                {
                    var categoryName = string.IsNullOrEmpty(c.Key) ? "unknown" : c.Key;
                    Console.WriteLine("".PadLeft(5) + $"{categoryName} : {c.Value.Sum(e => e.Amount)}");
                    if (detailed)
                    {
                        foreach (var e in c.Value.OrderBy(x => x.Date))
                        {
                            var source = e.Source?.ToString() ?? "";
                            if (source.Length > 43)
                            {
                                source = source.Substring(0, 40) + "...";
                            }

                            source = source.PadLeft(45);

                            Console.WriteLine("".PadLeft(10) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {source} {e.Amount.ToString("00.00").PadLeft(10)} {e.Category?.ToString().PadLeft(10)}");
                        }
                    }
                }
            }
        }

        public override BaseDataItemService<Expense> Service { get; set; }

        private ExpensesService expenseService;
    }
}