using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : BaseDataItemsController<Transaction>
    {
        private readonly ITransactionsService service;

        public ExpensesController(ITransactionsService service) : base(service)
        {
            this.service = service;
        }

        // GET api/values
        [Route("by-months-categories")]
        public virtual ActionResult<Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>>> GetByMonthsCategories()
        {
            return new JsonResult(this.service.GetExpensesByCategoriesByMonths(DateTime.Now.AddYears(-1), DateTime.MaxValue));
        }
    }
}