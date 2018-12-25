using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExpenseTracker.ConsoleClient
{
    internal class ItemEditorMenu : MenuBase
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
            var newValue = Utils.PromptInput("Enter prop value: ", propValues);
            this.editor.SetPropVal(p, newValue);
        }

        private ItemEditor editor;
    }
}