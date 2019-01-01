using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.RestClient
{
    public class BudgetRestClient : DataItemRestClient<Budget>, IBudgetService
    {
        public const string EndpointPath = "/api/budget";
        public BudgetRestClient(string baseAddress) : base(baseAddress, EndpointPath)
        {
        }

        public Budget GetByMonth(DateTime month)
        {
            var response = this.client.GetAsync(EndpointPath + $"/by-month/{month.Year}/{month.Month}").Result;
            ValidateResult(response);
            return this.Deserialize<Budget>(response.Content.ReadAsStreamAsync().Result);
        }
    }
}
