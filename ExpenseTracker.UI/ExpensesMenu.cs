using ExpenseTracker.Core;
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
            var amount = decimal.Parse(this.Renderer.PromptInput("Amount: ", "0"));
            var cat = this.Renderer.PromptInput("Category: ", string.Empty);
            var desc = this.Renderer.PromptInput("Description: ", string.Empty);
            var type = this.Renderer.PromptInput("Type: ", serializer.Serialize(TransactionType.Expense));
            var date = DateTime.Parse(this.Renderer.PromptInput("Date: ", DateTime.Now.ToString()));
            var save = this.Renderer.PromptInput("Save: ", "y");
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

        private void WriteExpensesByCategoriesByMonths(bool detailed)
        {
            var year = DateTime.Now.Year;
            var fromDate = new DateTime(year, 1, 1);
            var toDate = DateTime.Now.SetToEndOfMonth();
            this.Renderer.GetDateFilter(ref fromDate, ref toDate);

            var currentMonthDate = fromDate;
            while (currentMonthDate <= toDate.SetToEndOfMonth())
            {
                this.Renderer.WriteLine();
                this.Renderer.WriteLine($"{currentMonthDate.ToString("MMMM yyyy")}");

                this.WriteMonthSummary(currentMonthDate, 5);

                this.WriteCategoriesForMonth(detailed, currentMonthDate, 5);

                currentMonthDate = currentMonthDate.AddMonths(1);
            }

            this.WritePeriodSummary(fromDate, toDate);
            this.Renderer.WriteLine();
        }

        private void WriteCategoriesForMonth(bool detailed, DateTime currentMonthDate, int pad = 0)
        {
            var monthCategories = this.expenseService
                                .GetExpensesByCategoriesByMonths(currentMonthDate.SetToBeginningOfMonth(), currentMonthDate.SetToEndOfMonth())
                                .FirstOrDefault();

            if (monthCategories.Value != null)
            {
                this.Renderer.WriteLine();

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
            if (source.Length > 43)
            {
                source = source.Substring(0, 40) + "...";
            }

            source = source.PadLeft(45);

            this.Renderer.WriteLine("".PadLeft(padding) + $"{e.Id.ToString().PadRight(5)} {e.Date.ToString("dd ddd HH:mm").PadLeft(15)} {source} {e.Amount.ToString("F0").PadLeft(10)} {e.Category?.ToString().PadLeft(10)}");
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

            this.Renderer.Write($"{"".PadLeft(pad)}{categoryName} : ");
            if (catExpected != null && shouldRenderBudget)
            {
                this.Renderer.RenderActualExpectedNewLine(categoryActual, catExpected.Value);
            }
            else
            {
                this.Renderer.WriteLine($"{categoryActual.ToString("F0")}");
            }
        }

        private void WriteMonthSummary(DateTime month, int pad)
        {
            this.Renderer.WriteLine();

            var monthBudget = this.budgetService.GetCumulativeForMonth(month);
            var actualExpenses = this.budgetCalculator.CalculateActualExpenses(month.SetToBeginningOfMonth(), month.SetToEndOfMonth());
            var actualSavings = this.budgetCalculator.CalculateActualSavings(month.SetToBeginningOfMonth(), month.SetToEndOfMonth());
            var actualIncome = this.budgetCalculator.CalculateActualIncome(month.SetToBeginningOfMonth(), month.SetToEndOfMonth());

            if (monthBudget != null && month.SetToBeginningOfMonth() >= DateTime.Now.SetToBeginningOfMonth())
            {
                var isCurrentMonth = month.SetToBeginningOfMonth() == DateTime.Now.SetToBeginningOfMonth();
                var expectedExpenses = this.budgetCalculator.CalculateExpectedExpenses(monthBudget);
                var expectedSavings = this.budgetCalculator.CalculateExpectedSavings(monthBudget);
                var expectedIncome = this.budgetCalculator.CalculateExpectedIncome(monthBudget);

                this.Renderer.Write($"{"".PadLeft(pad)}Expenses: ");
                this.Renderer.RenderActualExpectedNewLine(actualExpenses, expectedExpenses, true, isCurrentMonth);
                if (!isCurrentMonth)
                {
                    this.Renderer.Write($"{"".PadLeft(pad)}Income: ");
                    this.Renderer.RenderActualExpectedNewLine(actualIncome, expectedIncome, false, false);
                    this.Renderer.Write($"{"".PadLeft(pad)}Savings: ");
                    this.Renderer.RenderActualExpectedNewLine(actualSavings, expectedSavings, false, false);
                }
            }
            else
            {
                this.Renderer.WriteLine($"{"".PadLeft(pad)}Expenses: {actualExpenses}");
                this.Renderer.WriteLine($"{"".PadLeft(pad)}Income: {actualIncome}");
                this.Renderer.Write($"{"".PadLeft(pad)}Savings: ");
                this.Renderer.RenderDiff(actualSavings);
                this.Renderer.WriteLine();
            }
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

            this.Renderer.WriteLine();
            this.Renderer.Write("Expected Savings: ");
            this.Renderer.RenderDiff(actualSavings + expectedSavings);

            this.Renderer.WriteLine();
            this.Renderer.Write("Saved so far: ");
            this.Renderer.RenderDiff(actualSavings);
        }
    }
}