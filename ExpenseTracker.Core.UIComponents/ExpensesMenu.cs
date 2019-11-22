using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.UI;

namespace ExpenseTracker.Core.UI
{
    public class ExpensesMenu : DataItemMenuBase<Transaction>
    {
        private readonly ITransactionsService expenseService;

        private readonly IBudgetService budgetService;
        private readonly IBudgetCalculator budgetCalculator;

        public ExpensesMenu(ITransactionsService expensesService, IBudgetService budgetService, IBudgetCalculator budgetCalculator)
        {
            this.Service = expensesService;
            this.expenseService = expensesService;
            this.budgetService = budgetService;
            this.budgetCalculator = budgetCalculator;
        }

        public override IBaseDataItemService<Transaction> Service { get; set; }

        public override string CommandKey => "ex";

        [MenuAction("sec", "Show expenses (categories only)")]
        public void ShowExpensesCategoriesOnly()
        {
            this.WriteExpensesByCategoriesByMonths(false);
        }

        [MenuAction("sea", "Show expenses (all)")]
        public void ShowExpensesAll()
        {
            this.WriteExpensesByCategoriesByMonths(true);
        }

        [MenuAction("cl", "Classify all expenses")]
        public void Categorize()
        {
            this.expenseService.Classify();
        }

        [MenuAction("qa", "Quick add expense")]
        public void QuickAddExpense()
        {
            var serializer = new Serializer();
            var amount = decimal.Parse(this.PromptInput("Amount: ", "0"));
            var cat = this.PromptInput("Category: ", string.Empty);
            var desc = this.PromptInput("Description: ", string.Empty);
            var type = this.PromptInput("Type: ", serializer.Serialize(TransactionType.Expense));
            var date = DateTime.Parse(this.PromptInput("Date: ", DateTime.Now.ToString()));
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
                    Source = desc,
                    Type = (TransactionType)serializer.Deserialize(typeof(TransactionType), type),
                    Date = date
                }
            });
        }

        [MenuAction("si", "Show income")]
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

        private void WriteExpensesByCategoriesByMonths(bool detailed)
        {
            var year = DateTime.Now.Year;
            var fromDate = DateTime.Now.SetToBeginningOfMonth();
            var toDate = DateTime.Now.SetToEndOfMonth();
            this.PromptDateFilter(ref fromDate, ref toDate);

            var currentMonthDate = fromDate;
            while (currentMonthDate <= toDate.SetToEndOfMonth())
            {
                this.Output.NewLine();
                this.Output.Write($"{currentMonthDate.ToString("MMMM yyyy")}");
                if (IsCurrentMonth(currentMonthDate))
                {
                    this.Output.Write(" (Current Month)", Style.MoreInfo);
                }

                this.Output.WriteLine("");

                this.WriteMonthSummary(currentMonthDate, 5);

                this.WriteCategoriesForMonth(detailed, currentMonthDate, 5);

                currentMonthDate = currentMonthDate.AddMonths(1);
            }

            this.WritePeriodSummary(fromDate, toDate);
            this.Output.NewLine();
        }

        private void WriteCategoriesForMonth(bool detailed, DateTime currentMonthDate, int pad = 0)
        {
            var monthCategories = this.expenseService
                                .GetExpensesByCategoriesByMonths(currentMonthDate.SetToBeginningOfMonth(), currentMonthDate.SetToEndOfMonth())
                                .FirstOrDefault();

            if (monthCategories.Value != null)
            {
                this.Output.NewLine();

                foreach (var category in monthCategories.Value.OrderBy(x => x.Key))
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

        private void WriteTransaction(Transaction e, int padding = 0)
        {
            var source = e.Source?.ToString() ?? "";
            if (source.Length > 110)
            {
                source = source.Substring(0, 110) + "...";
            }

            source = source.PadLeft(45);

            this.Output.WriteLine("".PadLeft(padding) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {e.Amount.ToString("F0").PadLeft(10)} {source} {e.Category?.ToString().PadLeft(10)}");
        }

        private void WriteCategoryForMonth(DateTime currentMonthDate, KeyValuePair<string, IEnumerable<Transaction>> category, int pad = 0)
        {
            var monthBudget = this.budgetService.GetCumulativeForMonth(currentMonthDate);
            var categoryName = string.IsNullOrEmpty(category.Key) ? "unknown" : category.Key;
            var categoryActual = category.Value.Sum(e => e.Amount);
            var budgetCategoryExists = monthBudget?.ExpectedTransactions.Any(x => x.Category == category.Key && x.Type == TransactionType.Expense);
            var catExpected = budgetCategoryExists.HasValue && budgetCategoryExists.Value ?
                monthBudget?.ExpectedTransactions.Where(x => x.Category == category.Key && x.Type == TransactionType.Expense).Sum(x => x.Amount) : null;
            var shouldRenderBudget = monthBudget != null && monthBudget.FromMonth.SetToBeginningOfMonth() <= DateTime.Now && DateTime.Now <= monthBudget.ToMonth;

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

        private void WriteMonthSummary(DateTime month, int pad)
        {
            this.Output.NewLine();

            var monthBudget = this.budgetService.GetCumulativeForMonth(month);
            var actualExpenses = this.budgetCalculator.CalculateActualExpenses(month.SetToBeginningOfMonth(), month.SetToEndOfMonth());
            var actualSavings = this.budgetCalculator.CalculateActualSavings(month.SetToBeginningOfMonth(), month.SetToEndOfMonth());
            var actualIncome = this.budgetCalculator.CalculateActualIncome(month.SetToBeginningOfMonth(), month.SetToEndOfMonth());

            if (monthBudget != null && month.SetToBeginningOfMonth() >= DateTime.Now.SetToBeginningOfMonth())
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

        private static bool IsCurrentMonth(DateTime month)
        {
            return month.SetToBeginningOfMonth() == DateTime.Now.SetToBeginningOfMonth();
        }

        private void WritePeriodSummary(DateTime from, DateTime to)
        {
            var currentMonthDate = from;
            decimal actualSavings = 0;
            decimal expectedSavings = 0;
            while (currentMonthDate <= to.SetToEndOfMonth())
            {
                if (currentMonthDate.SetToBeginningOfMonth() >= DateTime.Now.SetToBeginningOfMonth())
                {
                    var budget = this.budgetService.GetCumulativeForMonth(currentMonthDate);
                    if (budget != null)
                    {
                        expectedSavings += this.budgetCalculator.CalculateExpectedSavings(budget);
                    }
                }
                else
                {
                    actualSavings += this.budgetCalculator.CalculateActualSavings(currentMonthDate.SetToBeginningOfMonth(), currentMonthDate.SetToEndOfMonth());
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
    }
}