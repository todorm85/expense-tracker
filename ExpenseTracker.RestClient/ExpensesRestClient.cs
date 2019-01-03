using ExpenseTracker.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.RestClient
{
    public class ExpensesRestClient : DataItemRestClient<Expense>, IExpensesService
    {
        public ExpensesRestClient(IHttpClient client) : base(client)
        {
            this.EndpointPath = "/api/expenses";
        }

        public void Classify()
        {
            this.client.Put($"{this.EndpointPath}/classify", null);
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>> GetExpensesByCategoriesByMonths(
            DateTime fromDate, DateTime toDate)
        {
            var json = this.client.Get($"{this.EndpointPath}/by-months-categories");
            var dataObj = JsonConvert.DeserializeObject<Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>>>(json);
            return dataObj;
        }
    }
}