﻿using ExpenseTracker.Core;
using ExpenseTracker.UI;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core.UI
{
    public class BudgetMenu : DataItemMenuBase<Budget>
    {
        public BudgetMenu(IBudgetService service, ITransactionsService expensesService, IBudgetCalculator calculator)
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
            this.GetDateFilter(ref fromDate, ref toDate);

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
            //this.WriteSummary(budgets);
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

        private readonly IBudgetService budgetService;
        private readonly ITransactionsService expensesService;
        private readonly IBudgetCalculator calculator;
    }
}