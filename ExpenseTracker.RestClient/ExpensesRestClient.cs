using ExpenseTracker.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;

namespace ExpenseTracker.RestClient
{
    public class ExpensesRestClient : DataItemRestClient<Expense>, IExpensesService
    {
        private const string EndpointPath = "/api/expenses";

        public ExpensesRestClient(string baseAddress) : base(baseAddress, EndpointPath)
        {
        }

        public void Classify()
        {
            throw new NotImplementedException();
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>> GetExpensesByCategoriesByMonths(
            DateTime fromDate, DateTime toDate)
        {
            var response = this.client.GetAsync($"{EndpointPath}/by-months-categories").Result;
            ValidateResult(response);
            var json = response.Content.ReadAsStringAsync().Result;
            var dataObj = JsonConvert.DeserializeObject<Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>>>(json);
            return dataObj;
        }
    }
}