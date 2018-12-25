using System;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class BudgetMenu : MenuBase
    {
        private BudgetService service;

        public BudgetMenu()
        {
            this.service = ServicesFactory.GetService<BudgetService>();
        }
    }
}