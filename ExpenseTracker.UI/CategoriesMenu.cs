using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.UI
{
    public class CategoriesMenu : DataItemMenuBase<Category>
    {
        public CategoriesMenu(IBaseDataItemService<Category> service, IOutputRenderer renderer) : base(renderer)
        {
            this.Service = service;
        }

        public override IBaseDataItemService<Category> Service { get; set; }

        [MenuAction("sg", "Show groups")]
        public void ShowAll()
        {
            var groupByCats = this.Service.GetAll().GroupBy(x => x.Name);
            foreach (var gbc in groupByCats)
            {
                Renderer.WriteLine($"{gbc.Key}");
                foreach (var cat in gbc)
                {
                    Renderer.WriteLine("".PadLeft(5) + $"({cat.Id}) " + cat.ExpenseSourcePhrase);
                }
            }
        }

        [MenuAction("ij", "Import from JSON")]
        public void Import()
        {
            var path = Renderer.PromptInput("Enter path to file.");
            if (!File.Exists(path))
            {
                Renderer.WriteLine("Path not found.");
                return;
            }

            var keyCategory = new CategoriesJsonParser().ParseFile(path);
            var cats = new List<Category>();
            foreach (var kc in keyCategory)
            {
                cats.Add(new Category()
                {
                    ExpenseSourcePhrase = kc.Key,
                    Name = kc.Value
                });
            }

            this.Service.Add(cats);
        }
    }
}