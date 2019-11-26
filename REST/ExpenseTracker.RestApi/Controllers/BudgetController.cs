using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ExpenseTracker.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController : BaseDataItemsController<Budget>
    {
        private readonly IBudgetService service;

        public BudgetController(IBudgetService service) : base(service)
        {
            this.service = service;
        }

        [Route("by-month/{year}/{month}")]
        public ActionResult<Budget> GetByMonth(int year, int month)
        {
            return new JsonResult(this.service.GetCumulativeForMonth(new DateTime(year, month, 1)));
        }
    }
}