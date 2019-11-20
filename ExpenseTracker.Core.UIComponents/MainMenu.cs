using System;
using System.Collections.Generic;
using System.Text;
using ExpenseTracker.UI;

namespace ExpenseTracker.Core.UI
{
    public class MainMenu : Menu
    {
        public MainMenu(IOutputProvider output, IInputProvider input) : base(output, input)
        {
            this.Children = new Type[] { typeof(CategoriesMenu), typeof(BudgetMenu), typeof(ExpensesMenu) };
        }
    }
}
