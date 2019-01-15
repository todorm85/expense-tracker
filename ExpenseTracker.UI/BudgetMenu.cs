using System;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.UI
{
    public class BudgetMenu : DataItemMenuBase<Budget>
    {
        public BudgetMenu(IBudgetService service, IOutputRenderer renderer, IExpensesService expensesService, IBudgetCalculator calculator, ISalaryService salaryService) : base(renderer)
        {
            this.budgetService = service;
            this.expensesService = expensesService;
            this.calculator = calculator;
            this.salaryService = salaryService;
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

                var expectedIncome = this.calculator.CalculateExpectedIncome(budget);
                var expectedExpense = this.calculator.CalculateExpectedExpenses(budget);
                var expectedSavings = this.calculator.CalculateExpectedSavings(budget);
                var actualIncome = budget.ActualIncome;

                if (actualIncome != 0)
                {
                    var actualExpense = this.calculator.CalculateActualExpenses(budget);
                    var actualSaving = this.calculator.CalculateActualSavings(budget);
                    var savingsDiff = actualSaving - expectedSavings;

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

        [MenuAction("sal", "Edit salary")]
        public void EditSalary()
        {
            var newSalary = decimal.Parse(this.Renderer.PromptInput("Edit salary", this.salaryService.SalaryAmount.ToString()));
            this.salaryService.SalaryAmount = newSalary;
        }

        private void WriteSummary(IOrderedEnumerable<Budget> budgets)
        {
            var budgetsWithActualIncome = budgets.Where(b => b.ActualIncome != 0);
            var budgetsWithNoActualIncome = budgets.Where(b => b.ActualIncome == 0);

            var totalSavings = budgetsWithActualIncome.Sum(b => b.ActualIncome - this.calculator.CalculateActualExpenses(b));
            var totalExpectedSavings = budgetsWithNoActualIncome.Sum(b => this.calculator.CalculateExpectedSavings(b));

            this.Renderer.WriteLine($"Savings Summary: saved:{totalSavings} expected:{totalExpectedSavings} total:{totalSavings + totalExpectedSavings}");
            this.Renderer.WriteLine();
        }


        private readonly IBudgetService budgetService;
        private readonly IExpensesService expensesService;
        private readonly IBudgetCalculator calculator;
        private readonly ISalaryService salaryService;
    }
}