using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    class QueryMenu
    {
        public QueryMenu(ExpensesService svc)
        {
            this.service = svc;
        }

        public void Run()
        {
            string response = null;
            while (response != "e")
            {
                Console.WriteLine(@"
se: show expenses (by month)
sc: show categories (by month)
ed: edit
e: end");

                response = Console.ReadLine();
                switch (response)
                {
                    case "se":
                        ShowExpensesByMonth();
                        break;
                    case "sc":
                        ShowCategoriesByMonth();
                        break;
                    case "ed":
                        Edit();
                        break;
                    default:
                        break;
                }
            }
        }

        private void Edit()
        {
            var id = int.Parse(Utils.Prompt("Enter id to edit:"));
            var expense = this.service.GetAll().First(x => x.Id == id);
            expense.Source = Utils.Prompt("Edit expense source: ", expense.Source);
            this.service.Update(expense);
        }

        private void ShowCategoriesByMonth()
        {
            var categoriesByMonth = service.GetCategoriesCostByMonth(DateTime.Now.AddYears(-1), DateTime.MaxValue);
            foreach (var month in categoriesByMonth.OrderBy(x => x.Key))
            {
                Console.WriteLine(month.Key.ToString("MMMM yy"));
                foreach (var c in month.Value.OrderBy(x => x.Key))
                {
                    var categoryName = c.Key.PadLeft(10) ?? "".PadLeft(10);
                    Console.WriteLine($"{categoryName}      {c.Value}");
                }
            }
        }

        private void ShowExpensesByMonth()
        {
            var expensesByMonth = service.GetExpensesByMonths(DateTime.Now.AddYears(-1), DateTime.MaxValue);
            foreach (var month in expensesByMonth.OrderBy(x => x.Key))
            {
                Console.WriteLine(month.Key.ToString("MMMM yy"));
                foreach (var e in month.Value.OrderBy(x => x.Date))
                {
                    var source = e.Source?.ToString() ?? "";
                    if (source.Length > 43)
                    {
                        source = source.Substring(0, 40) + "...";
                    }

                    source = source.PadLeft(45);

                    Console.WriteLine($"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {source} {e.Amount.ToString("00.00").PadLeft(10)} {e.Category?.ToString().PadLeft(10)}");
                }
            }
        }

        private ExpensesService service;
    }
}
