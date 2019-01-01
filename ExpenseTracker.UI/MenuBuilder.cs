using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.UI
{
    public class MenuBuilder
    {
        private readonly IMenuFactory factory;
        private readonly IOutputRenderer renderer;

        public MenuBuilder(IMenuFactory factory, IOutputRenderer renderer)
        {
            this.factory = factory;
            this.renderer = renderer;
        }

        public virtual MenuBase Build()
        {
            var main = factory.Get<MenuBase>();
            main.AddAction("c", () => "Categories menu", () => factory.Get<CategoriesMenu>().Run());
            main.AddAction("ex", () => "Expenses menu", () => factory.Get<ExpensesMenu>().Run());
            main.AddAction("bu", () => "Budget menu", () => factory.Get<BudgetMenu>().Run());
            return main;
        }
    }
}
