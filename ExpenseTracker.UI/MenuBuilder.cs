using Unity;

namespace ExpenseTracker.UI
{
    public class MenuBuilder
    {
        private readonly IUnityContainer unity;

        public MenuBuilder(IUnityContainer unity)
        {
            this.unity = unity;
        }

        public Menu Build<T>() where T : Menu
        {
            var main = this.unity.Resolve<T>();
            ResolveChildren(main);
            return main;
        }

        private void ResolveChildren(Menu main)
        {
            foreach (var child in main.Children)
            {
                var childMenu = this.unity.Resolve(child) as Menu;
                main.AddAction(childMenu.MenuCommandName, () => childMenu.MenuCommandDescription, () => childMenu.Run());
                this.ResolveChildren(childMenu);
            }
        }
    }
}
