using System;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.UI
{
    public class BudgetMenu : DataItemMenuBase<Budget>
    {
        public BudgetMenu(IBudgetService service, IOutputRenderer renderer, IExpensesService expensesService) : base(renderer)
        {
            this.budgetService = service;
            this.expensesService = expensesService;
            this.Service = service;
        }

        public override IBaseDataItemService<Budget> Service { get; set; }

        public override void Show()
        {
            var fromDate = new DateTime(DateTime.Now.Year, 1, 1);
            var toDate = new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);
            Renderer.GetDateFilter(ref fromDate, ref toDate);

            var budgets = this.budgetService.GetAll().Where(x => x.Month >= fromDate && x.Month <= toDate)
                .OrderBy(x => x.Month);
            foreach (var budget in budgets)
            {
                this.Renderer.WriteLine();
                this.Renderer.Write($"{budget.Month.ToString("MMMM yyyy")}");
                this.Renderer.WriteLine($" ({budget.Id})", Style.MoreInfo);
                var prefix = "  ";

                var expectedIncome = budget.ExpectedIncome;
                var expectedExpense = budget.GetExpectedExpenses();
                var expectedSavings = expectedIncome - expectedExpense;
                var actualIncome = budget.ActualIncome;

                if (actualIncome != 0)
                {
                    var actualExpense = this.expensesService.GetAll()
                        .Where(e => e.Date.Year == budget.Month.Year && e.Date.Month == budget.Month.Month).Sum(e => e.Amount);
                    var actualSaving = budget.ActualIncome - actualExpense;
                    var savingsDiff = actualSaving - expectedSavings;

                    //this.Renderer.Write($" {actualSaving}", actualSaving > 0 ? Style.Success : Style.Error);
                    //this.Renderer.WriteLine($" (diff: {savingsDiff})", savingsDiff >= 0 ? Style.Success : Style.Error);
                    this.Renderer.Write($"{prefix}Savings:");
                    this.Renderer.Write($" {expectedSavings}", expectedSavings >= 0 ? Style.Success : Style.Error);
                    this.Renderer.Write($" {actualSaving}", actualSaving >= 0 ? Style.Success : Style.Error);
                    this.Renderer.WriteLine($" {savingsDiff}", savingsDiff >= 0 ? Style.Success : Style.Error);

                    this.Renderer.RenderDiffernceNewLine(expectedIncome, actualIncome, $"{prefix}Income: ");
                    this.Renderer.RenderDiffernceNewLine(expectedExpense, actualExpense, $"{prefix}Expense: ", false);
                }
                else
                {
                    this.Renderer.Write($"{prefix}Savings:");
                    this.Renderer.WriteLine($" {expectedSavings}", expectedSavings >= 0 ? Style.Success : Style.Error);
                    this.Renderer.WriteLine($"{prefix}Income: {expectedIncome}");
                    this.Renderer.WriteLine($"{prefix}Expense: {expectedExpense}");
                }

                var editor = new ItemEditor(budget);
                this.Renderer.WriteLine(
                    $"{prefix}{prefix}Details: {editor.GetPropVal(typeof(Budget).GetProperty(nameof(Budget.ExpectedExpensesByCategory)))}", Style.MoreInfo);
            }

            this.Renderer.WriteLine();
            this.WriteSummary(budgets);
        }

        private void WriteSummary(IOrderedEnumerable<Budget> budgets)
        {
            var budgetsWithActualIncome = budgets.Where(b => b.ActualIncome != 0);
            var budgetsWithNoActualIncome = budgets.Where(b => b.ActualIncome == 0);

            var totalSavings = budgetsWithActualIncome.Sum(b => b.ActualIncome - this.GetActualExpenses(b));
            var totalExpectedSavings = budgetsWithNoActualIncome.Sum(b => b.GetExpectedSavings());

            this.Renderer.WriteLine($"Savings Summary: saved:{totalSavings} expected:{totalExpectedSavings} total:{totalSavings + totalExpectedSavings}");
            this.Renderer.WriteLine();
        }

        private decimal GetActualExpenses(Budget b)
        {
            return this.expensesService.GetAll()
                                    .Where(e => e.Date.Year == b.Month.Year && e.Date.Month == b.Month.Month).Sum(e => e.Amount);
        }

        private readonly IBudgetService budgetService;
        private readonly IExpensesService expensesService;
    }
}