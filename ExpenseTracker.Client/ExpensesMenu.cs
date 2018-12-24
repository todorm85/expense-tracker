using System;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExpensesMenu : MenuBase
    {
        public ExpensesMenu()
        {
            this.service = ServicesFactory.GetService<ExpensesService>();
        }

        [MenuAction("ed", "Edit expense")]
        public void Edit()
        {
            var id = int.Parse(Utils.PromptInput("Enter id to edit:"));
            var expense = this.service.GetAll().First(x => x.Id == id);
            expense.Source = Utils.PromptInput("Edit expense source: ", expense.Source);
            this.service.Update(expense);
        }

        [MenuAction("s", "Show expenses (by month)")]
        public void ShowExpenses()
        {
            ShowExpenses(false);
        }

        private void ShowExpenses(bool detailed)
        {
            var categoriesByMonth = this.service.GetExpensesByCategoriesByMonths(DateTime.Now.AddYears(-1), DateTime.MaxValue);
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

        [MenuAction("sd", "Show expenses details (by month)")]
        public void ShowExpensesDetailed()
        {
            this.ShowExpenses(true);
        }

        private ExpensesService service;
    }
}