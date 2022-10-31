using ExpenseTracker.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Pages.Shared.Components.CategoriesDataList
{
    public class CategoriesDataListViewComponent : ViewComponent
    {
        private readonly IExpensesService service;

        public CategoriesDataListViewComponent(IExpensesService service)
        {
            this.service = service;
        }

        public IViewComponentResult Invoke(string id)
        {
            var categories = service.GetAllCategories();
            ViewData["id"] = id;
            return View(categories);
        }
    }
}