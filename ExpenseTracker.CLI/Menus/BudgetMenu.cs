using System;
using System.Collections.Generic;
using ExpenseTracker.UI;

namespace ExpenseTracker.Core.UI
{
    public class BudgetMenu : DataItemMenuBase<Budget>
    {
        private readonly IBudgetService budgetService;

        private readonly ITransactionsService expensesService;

        private readonly IBudgetCalculator calculator;

        private DateTime fromDate;

        private DateTime toDate;

        public BudgetMenu(IBudgetService service, ITransactionsService expensesService, IBudgetCalculator calculator)
        {
            this.budgetService = service;
            this.expensesService = expensesService;
            this.calculator = calculator;
            this.Service = service;
            this.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            this.toDate = new DateTime(DateTime.Now.Year, 12, 1);
        }

        public override IBaseDataItemService<Budget> Service { get; set; }

        [MenuAction("s", "Show budgets.")]
        public void ShowRelevant()
        {
            var items = this.Service.GetAll(x => x.ToMonth.SetToEndOfMonth() >= DateTime.Now);
            this.Show(items);
        }

        [MenuAction("sc", "Show cumulative monthly budgets.")]
        public void ShowCumulative()
        {
            this.PromptDateFilter();
            var month = this.fromDate;
            var budgets = new List<Budget>();
            while (month <= this.toDate)
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

                this.Output.NewLine();
                this.Output.Write($"{budget.FromMonth.ToString("MMMM yyyy")}");
                var prefix = "  ";
                this.Output.Write($"{prefix}Savings:");
                this.Output.WriteLine($" {expectedSavings}", expectedSavings >= 0 ? Style.Success : Style.Error);
                this.Output.WriteLine($"{prefix}Income: {expectedIncome}");
                this.Output.WriteLine($"{prefix}Expense: {expectedExpense}");

                this.Output.WriteLine(
                    $"{prefix}{prefix}Details: {new Serializer().Serialize(budget.ExpectedTransactions)}", Style.MoreInfo);
            }

            this.Output.NewLine();
        }

        [MenuAction("qa", "Quick add.")]
        public void QuickAdd()
        {
            var budget = new Budget()
            {
                FromMonth = DateTime.Now,
                ToMonth = DateTime.Now,
            };

            var res = this.PromptInput("Define budget (fromDate-toDate;;transactions(category:source:type:amount;)", this.SerializeBudget(budget));
            var newBudget = this.Deserialize(res);
            if (this.Confirm())
            {
                this.budgetService.Add(new Budget[] { newBudget });
            }
        }

        private void PromptDateFilter()
        {
            this.PromptDateFilter(ref fromDate, ref toDate);
        }

        private Budget Deserialize(string res)
        {
            var dates = res.Split(new string[] { ";;" }, StringSplitOptions.None)[0].Split('-');
            var from = DateTime.Parse(dates[0]);
            var to = DateTime.Parse(dates[1]);
            var se = new Serializer();

            var transactionsInput = res.Split(new string[] { ";;" }, StringSplitOptions.None)[1];
            var transactions = se.Deserialize(typeof(List<Transaction>), transactionsInput) as List<Transaction>;
            var budget = new Budget()
            {
                FromMonth = from,
                ToMonth = to,
                ExpectedTransactions = transactions
            };

            return budget;
        }

        private string SerializeBudget(Budget budget)
        {
            var res = $"{budget.FromMonth.ToShortDateString()}-{budget.ToMonth.ToShortDateString()}";
            return $"{res};;";
        }
    }
}