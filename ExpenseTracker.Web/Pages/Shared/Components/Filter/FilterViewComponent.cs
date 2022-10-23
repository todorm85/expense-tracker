using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Web.Pages.Shared.Components.Filter
{
    public class FilterViewComponent : ViewComponent
    {
        private readonly ITransactionsService service;

        public FilterViewComponent(ITransactionsService service)
        {
            this.service = service;
        }

        public IViewComponentResult Invoke(FiltersViewModel model)
        {
            if (model == null)
            {
                model = new FiltersViewModel();
            }

            model.Init(this.service.GetAllCategories());
            return View(model);
        }
    }
}
