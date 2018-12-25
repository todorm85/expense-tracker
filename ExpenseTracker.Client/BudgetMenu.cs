using System;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class BudgetMenu : DataItemMenuBase<Budget>
    {
        private BudgetService budgetService;

        public BudgetMenu()
        {
            this.budgetService = ServicesFactory.GetService<BudgetService>();
            this.Service = this.budgetService;
        }

        public override BaseDataItemService<Budget> Service { get; set; }
    }
}