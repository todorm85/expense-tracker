using ExpenseTracker.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.RestClient
{
    public class ExpensesRestClient : DataItemRestClient<Transaction>, ITransactionsService
    {
        public ExpensesRestClient(IHttpClient client) : base(client)
        {
            this.EndpointPath = "/api/expenses";
        }

        public void Classify()
        {
            this.client.Put($"{this.EndpointPath}/classify", null);
        }

        public IEnumerable<Transaction> GetDuplicates(Transaction t)
        {
            throw new NotImplementedException();
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>> GetExpensesByCategoriesByMonths(
            DateTime fromDate, DateTime toDate)
        {
            var json = this.client.Get($"{this.EndpointPath}/by-months-categories");
            var dataObj = JsonConvert.DeserializeObject<Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>>>(json);
            return dataObj;
        }
    }
}