using System;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal abstract class DataItemMenuBase<T> : MenuBase where T : class, IDataItem
    {
        public abstract BaseDataItemService<T> Service { get; set; }

        [MenuAction("ed", "Edit by id")]
        public void Edit()
        {
            var id = int.Parse(Utils.PromptInput("Enter id to edit:"));
            var item = this.Service.GetAll().First(x => x.Id == id);

            var editor = new ItemEditorMenu(item);
            editor.Run();
            this.Service.Update(new T[] { item });
        }

        [MenuAction("add", "Add")]
        public void Add()
        {
            T item = Activator.CreateInstance<T>();
            var editor = new ItemEditorMenu(item);
            editor.Run();
            this.Service.Add(new T[] { item });
        }

        [MenuAction("rem", "Remove")]
        public void Remove()
        {
            var id = int.Parse(Utils.PromptInput("Enter id to edit:"));
            var item = this.Service.GetAll().First(x => x.Id == id);
            this.Service.Remove(new T[] { item });
        }

        [MenuAction("sa", "Show all")]
        public void Show()
        {
            var items = this.Service.GetAll();
            foreach (var item in items)
            {
                var props = item.GetType().GetProperties();
                foreach (var p in props)
                {
                    Console.Write($"{p.Name}:'{new ItemEditor(item).GetPropVal(p)}'\n");
                }

                Console.WriteLine("\n");
            }
        }
    }
}