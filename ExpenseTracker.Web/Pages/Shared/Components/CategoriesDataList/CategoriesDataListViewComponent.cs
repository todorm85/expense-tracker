using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Pages.Shared.Components.CategoriesDataList
{
    public class CategoriesDataListViewComponent : ViewComponent
    {
        private readonly ITransactionsService service;

        public CategoriesDataListViewComponent(ITransactionsService service)
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