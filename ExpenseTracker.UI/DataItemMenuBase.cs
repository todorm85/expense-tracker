using ExpenseTracker.Core;
using System;
using System.Linq;

namespace ExpenseTracker.UI
{
    public abstract class DataItemMenuBase<T> : MenuBase where T : class, IDataItem
    {
        public abstract IBaseDataItemService<T> Service { get; set; }

        public DataItemMenuBase(IOutputRenderer renderer) : base(renderer)
        {
        }

        [MenuAction("ed", "Edit by id")]
        public void Edit()
        {
            var id = int.Parse(this.Renderer.PromptInput("Enter id to edit:"));
            var item = this.Service.GetAll().First(x => x.Id == id);

            var editor = new ItemEditorMenu(item, this.Renderer);
            editor.Run();
            if (this.Renderer.Confirm())
                this.Service.Update(new T[] { item });
        }

        [MenuAction("add", "Add")]
        public void Add()
        {
            T item = Activator.CreateInstance<T>();
            var editor = new ItemEditorMenu(item, this.Renderer);
            editor.Run();
            if (this.Renderer.Confirm())
                this.Service.Add(new T[] { item });
        }

        [MenuAction("rem", "Remove")]
        public void Remove()
        {
            var id = int.Parse(this.Renderer.PromptInput("Enter id to edit:"));
            var item = this.Service.GetAll().First(x => x.Id == id);
            this.Service.Remove(new T[] { item });
        }

        [MenuAction("sa", "Show all")]
        public virtual void Show()
        {
            var items = this.Service.GetAll();
            foreach (var item in items)
            {
                var props = item.GetType().GetProperties();
                foreach (var p in props)
                {
                    this.Renderer.Write($"{p.Name}:'{new Serializer().Serialize(p.GetValue(item))}'\n");
                }

                this.Renderer.WriteLine("\n");
            }
        }
    }
}