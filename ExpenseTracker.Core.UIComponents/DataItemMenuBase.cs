using ExpenseTracker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;

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
            if(!int.TryParse(this.PromptInput("Enter id to edit:"), out int id))
            {
                return;
            }

            var item = this.Service.GetAll(x => x.Id == id).FirstOrDefault();
            if (item == null)
            {
                return;
            }

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
            var id = int.Parse(this.PromptInput("Enter id to remove:"));
            var item = this.Service.GetAll(x => x.Id == id).FirstOrDefault();
            this.Service.Remove(new T[] { item });
        }

        [MenuAction("sf", "Filter all", "DataItem actions")]
        public virtual void ShowFiltered()
        {
            var input = this.PromptInput("Enter filter (Id >= 1 and ToDate <= DateTime.Now):");
            var items = this.Service.GetAll().Where(input);
            Show(items);
        }

        [MenuAction("sa", "Show all", "DataItem actions")]
        public virtual void ShowAll()
        {
            var items = this.Service.GetAll();
            Show(items);
        }

        protected void Show(IEnumerable<T> items)
        {
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