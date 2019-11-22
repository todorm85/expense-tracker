using ExpenseTracker.UI;
using System;
using System.Linq;

namespace ExpenseTracker.Core.UI
{
    public abstract class DataItemMenuBase<T> : Menu where T : class, IDataItem
    {
        public abstract IBaseDataItemService<T> Service { get; set; }

        public DataItemMenuBase()
        {
            this.AddAction("Item type", () => $"{typeof(T).Name}", null, "DataItem actions", -1);
        }

        [MenuAction("ed", "Edit by id", "DataItem actions")]
        public void Edit()
        {
            var id = int.Parse(this.PromptInput("Enter id to edit:"));
            var item = this.Service.GetAll(x => x.Id == id).FirstOrDefault();

            var editor = new ItemEditorMenu(item);
            editor.Run();
            if (this.Confirm())
                this.Service.Update(new T[] { item });
        }

        [MenuAction("add", "Add", "DataItem actions")]
        public void Add()
        {
            T item = Activator.CreateInstance<T>();
            var editor = new ItemEditorMenu(item);
            editor.Run();
            if (this.Confirm())
                this.Service.Add(new T[] { item });
        }

        [MenuAction("rem", "Remove", "DataItem actions")]
        public void Remove()
        {
            var id = int.Parse(this.PromptInput("Enter id to edit:"));
            var item = this.Service.GetAll(x => x.Id == id).FirstOrDefault();
            this.Service.Remove(new T[] { item });
        }

        [MenuAction("sa", "Show all", "DataItem actions")]
        public virtual void Show()
        {
            var items = this.Service.GetAll();
            foreach (var item in items)
            {
                var props = item.GetType().GetProperties();
                foreach (var p in props)
                {
                    this.Output.Write($"{p.Name}:'{new Serializer().Serialize(p.GetValue(item))}'\n");
                }

                this.Output.WriteLine("\n");
            }
        }
    }
}