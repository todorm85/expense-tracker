using System;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.UI;
using Microsoft.Practices.Unity;

namespace ExpenseTracker.ConsoleClient
{
    internal class MenuFactory : IMenuFactory
    {
        private readonly IUnityContainer container;

        public MenuFactory(IUnityContainer container)
        {
            this.container = container;
        }

        public T Get<T>() where T : MenuBase
        {
            return container.Resolve<T>();
        }
    }
}