namespace ExpenseTracker.UI
{
    public class MenuBuilder
    {
        private readonly IMenuFactory unity;

        public MenuBuilder(IMenuFactory unity)
        {
            this.unity = unity;
        }

        public Menu Build<T>() where T : Menu
        {
            var main = this.unity.Create<T>(typeof(T));
            ResolveChildren(main);
            return main;
        }

        private void ResolveChildren(Menu main)
        {
            foreach (var childType in main.Children)
            {
                var childMenu = this.unity.Create<Menu>(childType);
                main.AddAction(childMenu.MenuCommandName, () => childMenu.MenuCommandDescription, () => childMenu.Run());
                this.ResolveChildren(childMenu);
            }
        }
    }
}
