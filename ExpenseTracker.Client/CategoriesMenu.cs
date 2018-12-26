using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class CategoriesMenu : DataItemMenuBase<Category>
    {
        public CategoriesMenu()
        {
            this.categoriesService = ServicesFactory.GetService<CategoriesService>();
            this.Service = this.categoriesService;
        }

        public override BaseDataItemService<Category> Service { get; set; }

        [MenuAction("sg", "Show groups")]
        public void ShowAll()
        {
            var groupByCats = this.categoriesService.GetAll().GroupBy(x => x.Name);
            foreach (var gbc in groupByCats)
            {
                Console.WriteLine($"{gbc.Key}");
                foreach (var cat in gbc)
                {
                    Console.WriteLine("".PadLeft(5) + $"({cat.Id}) " + cat.ExpenseSourcePhrase);
                }
            }
        }

        [MenuAction("ij", "Import from JSON")]
        public void Import()
        {
            Console.WriteLine("Enter path to file.");
            var path = Console.ReadLine();
            if (!File.Exists(path))
            {
                Console.WriteLine("Path not found.");
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

            this.categoriesService.Add(cats);
        }

        private readonly CategoriesService categoriesService;
    }
}