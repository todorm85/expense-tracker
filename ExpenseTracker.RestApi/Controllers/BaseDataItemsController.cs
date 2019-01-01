using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.RestApi.Controllers
{
    public class BaseDataItemsController<T> : ControllerBase where T : IDataItem
    {
        private readonly IBaseDataItemService<T> service;

        public BaseDataItemsController(IBaseDataItemService<T> service)
        {
            this.service = service;
        }

        // GET api/values
        [HttpGet]
        public virtual ActionResult<IEnumerable<T>> Get()
        {
            return new JsonResult(this.service.GetAll());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public virtual ActionResult<string> Get(int id)
        {
            return new JsonResult(this.service.GetAll().FirstOrDefault(x => x.Id == id));
        }

        // POST api/values
        [HttpPost]
        public virtual void Post([FromBody] IEnumerable<T> value)
        {
            this.service.Add(value);
        }

        // PUT api/values/5
        public virtual void Put([FromBody] IEnumerable<T> value)
        {
            this.service.Update(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public virtual void Delete(int id)
        {
            var toRemove = this.service.GetAll().FirstOrDefault(x => x.Id == id);
            this.service.Remove(new T[] { toRemove });
        }
    }
}
