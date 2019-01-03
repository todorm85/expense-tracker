using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.RestClient
{
    public class BudgetRestClient : DataItemRestClient<Budget>, IBudgetService
    {
        public const string EndpointPath = "/api/budget";
        public BudgetRestClient(IHttpClient client) : base(client)
        {
        }

        public Budget GetByMonth(DateTime month)
        {
            var response = this.client.Get(EndpointPath + $"/by-month/{month.Year}/{month.Month}");
            return this.Deserialize<Budget>(response);
        }
    }
}
