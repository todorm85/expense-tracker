using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.UI;
using Unity;

namespace ExpenseTracker.ConsoleClient
{
    class MenuFactory : IMenuFactory
    {
        private readonly IUnityContainer container;

        public MenuFactory(IUnityContainer container)
        {
            this.container = container;
        }

        public T Create<T>(Type type) where T : Menu
        {
            return this.container.Resolve(type) as T;
        }
    }
}
