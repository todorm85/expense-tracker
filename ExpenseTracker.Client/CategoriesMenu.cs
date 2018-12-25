using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class CategoriesMenu : MenuBase
    {
        public CategoriesMenu()
        {
            this.service = ServicesFactory.GetService<CategoriesService>();
        }

        [MenuAction("d", "Delete")]
        public void Delete()
        {
            Console.WriteLine("Enter source phrase to match:");
            var phrase = Console.ReadLine();
            this.service.Remove(phrase);
        }

        [MenuAction("i", "Insert")]
        public void Insert()
        {
            Console.WriteLine("Enter source phrase to match:");
            var phrase = Console.ReadLine();
            Console.WriteLine("Enter category name:");
            var cn = Console.ReadLine();
            this.service.Add(new Category[] { new Category()
            {
                ExpenseSourcePhrase = phrase,
                Name = cn
            } });
        }

        [MenuAction("s", "Show all")]
        public void ShowAll()
        {
            var groupByCats = this.service.GetAll().GroupBy(x => x.Name);
            foreach (var gbc in groupByCats)
            {
                Console.WriteLine($"{gbc.Key}");
                foreach (var cat in gbc)
                {
                    Console.WriteLine("".PadLeft(5) + cat.ExpenseSourcePhrase);
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

            this.service.Add(cats);
        }

        private CategoriesService service;
    }
}