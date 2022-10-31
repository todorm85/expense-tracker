using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly IExpensesService service;

        [BindProperty]
        public string CategoryName { get; set; }

        [BindProperty]
        public string NewCategoryName { get; set; }

        public IEnumerable<string> AllCategories { get; set; }

        public IndexModel(IExpensesService service)
        {
            this.service = service;
            AllCategories = service.GetAllCategories();
        }

        public void OnPostRename()
        {
            this.service.RenameCategory(CategoryName, NewCategoryName);
            this.ModelState.Clear();
            this.CategoryName = "";
            this.NewCategoryName = "";
        }
    }
}
