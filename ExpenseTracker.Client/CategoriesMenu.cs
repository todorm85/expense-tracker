using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    internal class CategoriesMenu
    {
        private CategoriesService service;

        public CategoriesMenu(IUnitOfWork unitOfWork)
        {
            this.service = new CategoriesService(unitOfWork);
        }

        internal void Run()
        {
            string response = null;
            while (response != "e")
            {
                Console.WriteLine(@"
im: import from json
i: insert
s: show all
d: delete
e: end");

                response = Console.ReadLine();
                switch (response)
                {
                    case "im":
                        this.Import();
                        break;
                    case "s":
                        this.ShowAll();
                        break;
                    case "i":
                        this.Insert();
                        break;
                    case "d":
                        this.Delete();
                        break;
                    default:
                        break;
                }
            }
        }

        private void Delete()
        {
            Console.WriteLine("Enter source phrase to match:");
            var phrase = Console.ReadLine();
            this.service.Delete(phrase);
        }

        private void Insert()
        {
            Console.WriteLine("Enter source phrase to match:");
            var phrase = Console.ReadLine();
            Console.WriteLine("Enter category name:");
            var cn = Console.ReadLine();
            this.service.Insert(new Category[] { new Category()
            {
                ExpenseSourcePhrase = phrase,
                Name = cn
            } });
        }

        private void ShowAll()
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

        private void Import()
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

            this.service.Insert(cats);
        }

    }
}