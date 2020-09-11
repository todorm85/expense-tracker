using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages
{
    [BindProperties]
    public class CategoriesModel : PageModel
    {
        private readonly CategoriesService service;

        public CategoriesModel(CategoriesService service)
        {
            this.service = service;
        }

        public IList<Category> Categories { get; set; }

        public Category CreateCategory { get; set; }

        public void OnGet()
        {
            this.Categories = this.service.GetAll().OrderBy(x => x.Name).ToList();
        }

        public IActionResult OnPostCreate()
        {
            this.service.Add(new Category[] { this.CreateCategory });
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int id)
        {
            var cat = this.service.GetAll(x => x.Id == id).First();
            this.service.Remove(new Category[] { cat });
            return RedirectToPage();
        }
    }
}
