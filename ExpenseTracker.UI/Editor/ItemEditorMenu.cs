using System.Reflection;

namespace ExpenseTracker.UI
{
    public class ItemEditorMenu : Menu
    {
        public ItemEditorMenu(object item)
        {
            this.editor = new ItemEditor(item);
            var props = this.editor.GetProperties();
            var i = 1;
            foreach (var p in props)
            {
                this.AddAction(i.ToString(), () => $"{p.Name}:{this.editor.GetPropVal(p)}", () => this.Edit(p));
                i++;
            }
        }

        private void Edit(PropertyInfo p)
        {
            var propValues = this.editor.GetPropVal(p);
            var newValue = this.PromptInput($"Enter prop value{p.GetAdditionalInfo()}: ", propValues);
            this.editor.SetPropVal(p, newValue);
        }

        private ItemEditor editor;
    }
}