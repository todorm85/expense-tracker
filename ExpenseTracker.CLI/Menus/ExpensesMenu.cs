using ExpenseTracker.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core.UI
{
    public class ExpensesMenu : DataItemMenuBase<Transaction>
    {
        private const string UnknownCategory = "n/a";
        private readonly IBudgetCalculator budgetCalculator;
        private readonly IBudgetService budgetService;
        private readonly CategoriesMenu catMenu;
        private readonly ITransactionsService expenseService;
        private string categoryFilter;
        private DateTime fromDate = DateTime.Now.ToMonthStart();
        private DateTime toDate = DateTime.Now.ToMonthEnd();

        public ExpensesMenu(
            ITransactionsService expensesService,
            IBudgetService budgetService,
            IBudgetCalculator budgetCalculator,
            CategoriesMenu catMenu)
        {
            this.Service = expensesService;
            this.expenseService = expensesService;
            this.budgetService = budgetService;
            this.budgetCalculator = budgetCalculator;
            this.catMenu = catMenu;
        }

        public override string CommandKey => "ex";
        public override IBaseDataItemService<Transaction> Service { get; set; }

        [MenuAction("cb", "Manually Categorize bulk")]
        public void CategorizeBulk()
        {
            var input = PromptReadIds();
            var cat = this.PromptInput("Enter category:");
            if (input.Count() == 0 || string.IsNullOrWhiteSpace(cat))
            {
                this.Output.WriteLine("Invalid ids entered or empty category");
                return;
            }

            var items = this.Service.GetAll(x => input.Contains(x.Id));
            foreach (var item in items)
            {
                item.Category = cat;
                this.Service.Update(new Transaction[] { item });
            }
        }

        [MenuAction("ec", "Edit category", "Categories")]
        public void EditCategory()
        {
            this.catMenu.Edit();
        }

        [MenuAction("gd", "Get duplicates", "queries")]
        public void GetDuplicates()
        {
            var duplicateIds = new List<int>();
            foreach (var item in this.expenseService.GetAll())
            {
                if (duplicateIds.Contains(item.Id))
                {
                    continue;
                }

                var duplicates = this.expenseService.GetDuplicates(item);
                if (duplicates.Count() > 1)
                {
                    this.Output.Write("Duplicates: ");
                    foreach (var duplicate in duplicates)
                    {
                        this.WriteTransaction(duplicate);
                        duplicateIds.Add(duplicate.Id);
                    }
                }
            }
        }

        [MenuAction("ib", "Ignore bulk")]
        public void IgnoreBulk()
        {
            var ids = PromptReadIds();
            foreach (var id in ids)
            {
                var item = this.Service.GetAll(x => x.Id == id).First();
                item.Ignored = true;
                this.Service.Update(new Transaction[] { item });
            }
        }

        [MenuAction("ibc", "Ignore bulk in category")]
        public void IgnoreBulkCategory()
        {
            var input = this.PromptInput("Category to ignore items of:");
            if (string.IsNullOrWhiteSpace(input))
            {
                this.Output.WriteLine("Invalid category");
                return;
            }

            var items = this.Service.GetAll(x => x.Category == input);
            foreach (var item in items)
            {
                item.Ignored = true;
                this.Service.Update(new Transaction[] { item });
            }
        }

        [MenuAction("qac", "Quick add category", "Categories")]
        public void QuickAddCat()
        {
            this.catMenu.QuickAdd();
        }

        [MenuAction("qa", "Quick add expense")]
        public void QuickAddExpense()
        {
            var input = this.PromptInput("Enter category (amount:date(month,day(optional)(def 1),year(optional)(def - now)):category(optional):description(optional):type(+ or -(def))(optional))").Split(':');
            if (input.Length < 2)
            {
                return;
            }

            var amount = decimal.Parse(input[0]);
            DateTime date = ParseDate(input[1]);
            var cat = string.Empty;
            if (input.Length > 2)
            {
                cat = input[2];
            }

            var desc = string.Empty;
            if (input.Length > 3)
            {
                desc = input[3];
            }

            var type = "-";
            if (input.Length > 4)
            {
                type = input[4];
            }

            var serializer = new Serializer();
            var save = this.PromptInput("Save: ", "y");
            if (save != "y")
            {
                return;
            }

            this.Service.Add(new Transaction[]
            {
                new Transaction()
                {
                    Amount = amount,
                    Category = cat,
                    Details = desc,
                    Type = (TransactionType)serializer.Deserialize(typeof(TransactionType), type),
                    Date = date
                }
            });
        }

        [MenuAction("cf", "Set category filter", "filters")]
        public void SetCategoryFilters()
        {
            this.categoryFilter = this.PromptInput("Enter category filter ");
        }

        [MenuAction("df", "Set date filter", "filters")]
        public void SetDateFilters()
        {
            this.PromptDateFilter(ref this.fromDate, ref this.toDate);
        }

        [MenuAction("scg", "Show category groups", "Categories")]
        public void ShowCategoryGroups()
        {
            this.catMenu.ShowAllCategoryGroups();
        }

        [MenuAction("s", "Show expenses by categories (detailed)", "queries")]
        public void ShowExpensesAll()
        {
            this.WriteExpensesByCategoriesByMonths(true);
        }

        [MenuAction("sc", "Show expenses by categories", "queries")]
        public void ShowExpensesCategoriesOnly()
        {
            this.WriteExpensesByCategoriesByMonths(false);
        }

        [MenuAction("si", "Show income", "queries")]
        public void ShowIncome()
        {
            var incomes = this.expenseService.GetAll(x => !x.Ignored && x.Type == TransactionType.Income);
            var byYears = incomes.GroupBy(x => x.Date.Year);
            foreach (var year in byYears)
            {
                var byMonths = year.GroupBy(x => x.Date.Month);
                foreach (var month in byMonths)
                {
                    this.Output.NewLine();
                    this.Output.WriteLine($"{new DateTime(year.Key, month.Key, 1).ToString("MMMM yyyy")}");

                    foreach (var transaction in month)
                    {
                        this.WriteTransaction(transaction, 5);
                    }
                }
            }
        }

        private static bool IsCurrentMonth(DateTime month)
        {
            return month.ToMonthStart() == DateTime.Now.ToMonthStart();
        }

        private static DateTime ParseDate(string dateString)
        {
            var dateParts = dateString.Split(',');
            var month = int.Parse(dateParts[0]);
            int day = 1;
            if (dateParts.Length > 1)
            {
                day = int.Parse(dateParts[1]);
            }

            int year = DateTime.Now.Year;
            if (dateParts.Length > 2)
            {
                year = int.Parse(dateParts[2]);
            }

            var date = new DateTime(year, month, day);
            return date;
        }

        private IEnumerable<int> PromptReadIds()
        {
            var input = this.PromptInput("Ids to ignore separated by space:");
            var ids = input.Split(' ');
            List<int> results = new List<int>();
            foreach (var id in ids)
            {
                results.Add(int.Parse(id));
            }

            return results;
        }

        private void WriteCategoriesForMonth(bool detailed, DateTime currentMonthDate, int pad = 0)
        {
            var monthCategories = this.expenseService
                                .GetExpensesByCategoriesByMonths(currentMonthDate.ToMonthStart(), currentMonthDate.ToMonthEnd())
                                .FirstOrDefault();

            if (monthCategories.Value != null)
            {
                this.Output.NewLine();

                foreach (var category in monthCategories.Value
                    .Where(x => (string.IsNullOrWhiteSpace(this.categoryFilter) || x.Key == this.categoryFilter) ||
                        (this.categoryFilter == UnknownCategory && string.IsNullOrWhiteSpace(x.Key)))
                    .OrderBy(x => x.Key))
                {
                    this.WriteCategoryForMonth(currentMonthDate, category, pad);
                    if (detailed)
                    {
                        foreach (var e in category.Value.OrderBy(x => x.Date))
                        {
                            this.WriteTransaction(e, pad * 2);
                        }
                    }
                }
            }
        }

        private void WriteCategoryForMonth(DateTime currentMonthDate, KeyValuePair<string, IEnumerable<Transaction>> category, int pad = 0)
        {
            var monthBudget = this.budgetService.GetCumulativeForMonth(currentMonthDate);
            var categoryName = string.IsNullOrEmpty(category.Key) ? UnknownCategory : category.Key;
            var categoryActual = category.Value.Sum(e => e.Amount);
            var budgetCategoryExists = monthBudget?.ExpectedTransactions.Any(x => x.Category == category.Key && x.Type == TransactionType.Expense);
            var catExpected = budgetCategoryExists.HasValue && budgetCategoryExists.Value ?
                monthBudget?.ExpectedTransactions.Where(x => x.Category == category.Key && x.Type == TransactionType.Expense).Sum(x => x.Amount) : null;
            var shouldRenderBudget = monthBudget != null && monthBudget.FromMonth.ToMonthStart() <= DateTime.Now && DateTime.Now <= monthBudget.ToMonth;

            this.Output.NewLine();
            this.Output.Write($"{"".PadLeft(pad)}{categoryName} : ");
            if (catExpected != null && shouldRenderBudget)
            {
                this.ShowActualExpectedNewLine(categoryActual, catExpected.Value);
            }
            else
            {
                this.Output.WriteLine($"{categoryActual.ToString("F0")}");
            }
        }

        private void WriteExpensesByCategoriesByMonths(bool detailed)
        {
            var currentMonthDate = this.fromDate;
            while (currentMonthDate <= this.toDate.ToMonthEnd())
            {
                this.Output.NewLine();
                this.Output.Write($"{currentMonthDate.ToString("MMMM yyyy")}");
                if (IsCurrentMonth(currentMonthDate))
                {
                    this.Output.Write(" (Current)", Style.MoreInfo);
                }

                this.Output.WriteLine("");

                this.WriteMonthSummary(currentMonthDate, 5);

                this.WriteCategoriesForMonth(detailed, currentMonthDate, 5);

                currentMonthDate = currentMonthDate.AddMonths(1);
            }

            this.WritePeriodSummary(this.fromDate, this.toDate);
            this.Output.NewLine();
        }

        private void WriteMonthSummary(DateTime month, int pad)
        {
            this.Output.NewLine();

            var monthBudget = this.budgetService.GetCumulativeForMonth(month);
            var actualExpenses = this.budgetCalculator.CalculateActualExpenses(month.ToMonthStart(), month.ToMonthEnd());
            var actualSavings = this.budgetCalculator.CalculateActualSavings(month.ToMonthStart(), month.ToMonthEnd());
            var actualIncome = this.budgetCalculator.CalculateActualIncome(month.ToMonthStart(), month.ToMonthEnd());

            if (monthBudget != null && month.ToMonthStart() >= DateTime.Now.ToMonthStart())
            {
                bool isCurrentMonth = IsCurrentMonth(month);
                var expectedExpenses = this.budgetCalculator.CalculateExpectedExpenses(monthBudget);
                var expectedSavings = this.budgetCalculator.CalculateExpectedSavings(monthBudget);
                var expectedIncome = this.budgetCalculator.CalculateExpectedIncome(monthBudget);

                this.Output.Write($"{"".PadLeft(pad)}Expenses: ");
                this.ShowActualExpected(actualExpenses, expectedExpenses, secondaryShouldBeHigher: true, renderDiff: isCurrentMonth);
                if (!isCurrentMonth)
                {
                    this.Output.NewLine();
                    this.Output.Write($"{"".PadLeft(pad)}Income: ");
                    this.ShowActualExpectedNewLine(actualIncome, expectedIncome, false, false);
                    this.Output.Write($"{"".PadLeft(pad)}Savings: ");
                    this.ShowActualExpected(actualSavings, expectedSavings, false, false);
                }
            }
            else
            {
                this.Output.WriteLine($"{"".PadLeft(pad)}Expenses: {actualExpenses}");
                this.Output.WriteLine($"{"".PadLeft(pad)}Income: {actualIncome}");
                this.Output.Write($"{"".PadLeft(pad)}Savings: ");
                this.ShowDiff(actualSavings);
            }
        }

        private void WritePeriodSummary(DateTime from, DateTime to)
        {
            var currentMonthDate = from;
            decimal actualSavings = 0;
            decimal expectedSavings = 0;
            while (currentMonthDate <= to.ToMonthEnd())
            {
                if (currentMonthDate.ToMonthStart() >= DateTime.Now.ToMonthStart())
                {
                    var budget = this.budgetService.GetCumulativeForMonth(currentMonthDate);
                    if (budget != null)
                    {
                        expectedSavings += this.budgetCalculator.CalculateExpectedSavings(budget);
                    }
                }
                else
                {
                    actualSavings += this.budgetCalculator.CalculateActualSavings(currentMonthDate.ToMonthStart(), currentMonthDate.ToMonthEnd());
                }

                currentMonthDate = currentMonthDate.AddMonths(1);
            }

            this.Output.NewLine();
            this.Output.Write("Expected Total Savings: ");
            this.ShowDiff(actualSavings + expectedSavings);

            this.Output.NewLine();
            this.Output.Write("Saved so far: ");
            this.ShowDiff(actualSavings);
        }

        private void WriteTransaction(Transaction e, int padding = 0)
        {
            var source = e.Details?.ToString() ?? "";
            if (source.Length > 110)
            {
                source = source.Substring(0, 110) + "...";
            }

            source = source.PadLeft(45);

            this.Output.WriteLine("".PadLeft(padding) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {e.Amount.ToString("F0").PadLeft(10)} {source} {e.Category?.ToString().PadLeft(10)}");
        }
    }
}