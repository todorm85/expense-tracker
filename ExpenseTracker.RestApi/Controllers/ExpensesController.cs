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
    public class ExpensesController : BaseDataItemsController<Expense>
    {
        private readonly IExpensesService service;

        public ExpensesController(IExpensesService service) : base(service)
        {
            this.service = service;
        }

        // GET api/values
        [Route("by-months-categories")]
        public virtual ActionResult<Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>>> GetByMonthsCategories()
        {
            return new JsonResult(this.service.GetExpensesByCategoriesByMonths(DateTime.Now.AddYears(-1), DateTime.MaxValue));
        }

        [Route("classify")]
        [HttpPut]
        public void Classsify()
        {
            this.service.Classify();
        }
    }
}