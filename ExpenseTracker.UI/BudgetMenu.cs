using ExpenseTracker.Core;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.UI
{
    public class BudgetMenu : DataItemMenuBase<Budget>
    {
        public BudgetMenu(IBudgetService service, IOutputRenderer renderer, ITransactionsService expensesService, IBudgetCalculator calculator) : base(renderer)
        {
            this.budgetService = service;
            this.expensesService = expensesService;
            this.calculator = calculator;
            this.Service = service;
        }

        public override IBaseDataItemService<Budget> Service { get; set; }

        [MenuAction("scu", "Show cumulative monthly budgets.")]
        public void ShowCumulative()
        {
            var fromDate = new DateTime(DateTime.Now.Year, 1, 1);
            var toDate = new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);
            this.Renderer.GetDateFilter(ref fromDate, ref toDate);

            //var budgets = this.budgetService.GetAll()
            //    .Where(x => x.FromMonth >= fromDate && x.ToMonth <= toDate)
            //    .OrderBy(x => x.FromMonth);

            var month = fromDate;
            var budgets = new List<Budget>();
            while (month <= toDate)
            {
                var budget = this.budgetService.GetCumulativeForMonth(month);
                if (budget != null)
                {
                    budgets.Add(budget);
                }

                month = month.AddMonths(1);
            }

            foreach (var budget in budgets)
            {
                var expectedIncome = this.calculator.CalculateExpectedIncome(budget);
                var expectedExpense = this.calculator.CalculateExpectedExpenses(budget);
                var expectedSavings = this.calculator.CalculateExpectedSavings(budget);

                this.Renderer.WriteLine();
                this.Renderer.Write($"{budget.FromMonth.ToString("MMMM yyyy")}");
                var prefix = "  ";
                this.Renderer.Write($"{prefix}Savings:");
                this.Renderer.WriteLine($" {expectedSavings}", expectedSavings >= 0 ? Style.Success : Style.Error);
                this.Renderer.WriteLine($"{prefix}Income: {expectedIncome}");
                this.Renderer.WriteLine($"{prefix}Expense: {expectedExpense}");

                var editor = new ItemEditor(budget);
                this.Renderer.WriteLine(
                    $"{prefix}{prefix}Details: {editor.GetPropVal(typeof(Budget).GetProperty(nameof(Budget.ExpectedTransactions)))}", Style.MoreInfo);
            }

            this.Renderer.WriteLine();
            //this.WriteSummary(budgets);
        }

        //private void WriteSummary(IOrderedEnumerable<Budget> budgets)
        //{
        //    var budgetsWithActualIncome = budgets.Where(b => b.ActualIncome != 0);
        //    var budgetsWithNoActualIncome = budgets.Where(b => b.ActualIncome == 0);

        //    var totalSavings = budgetsWithActualIncome.Sum(b => b.ActualIncome - this.calculator.CalculateActualExpenses(b));
        //    var totalExpectedSavings = budgetsWithNoActualIncome.Sum(b => this.calculator.CalculateExpectedSavings(b));

        //    this.Renderer.WriteLine($"Savings Summary: saved:{totalSavings} expected:{totalExpectedSavings} total:{totalSavings + totalExpectedSavings}");
        //    this.Renderer.WriteLine();
        //}


        private readonly IBudgetService budgetService;
        private readonly ITransactionsService expensesService;
        private readonly IBudgetCalculator calculator;
    }
}