using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.UI;

namespace ExpenseTracker.Core.UI
{
    public class CategoriesMenu : DataItemMenuBase<Category>
    {
        public CategoriesMenu(IBaseDataItemService<Category> service)
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
                this.Output.WriteLine($"{gbc.Key}");
                foreach (var cat in gbc)
                {
                    this.Output.WriteLine("".PadLeft(5) + $"({cat.Id}) " + cat.ExpenseSourcePhrase);
                }
            }
        }

        [MenuAction("ij", "Import from JSON")]
        public void Import()
        {
            var path = this.PromptInput("Enter path to file.");
            if (!File.Exists(path))
            {
                this.Output.WriteLine("Path not found.");
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