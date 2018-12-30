using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;

namespace ExpenseTracker.RestClient.IntTests
{
    [TestClass]
    public class ExpensesTests
    {
        [TestMethod]
        public void Expenses_Get()
        {
            var service = new ExpensesRestClient(new HttpClient() { BaseAddress = new Uri("https://localhost:44376") });
            var result = service.GetExpensesByCategoriesByMonths(DateTime.Now.AddYears(-1), DateTime.MaxValue);
        }

        [TestMethod]
        public void Expenses_Post()
        {
            var service = new ExpensesRestClient(new HttpClient() { BaseAddress = new Uri("https://localhost:44376") });
            service.Add(new Expense[] 
            {
                new Expense()
                {
                    Amount = 5,
                    Date = DateTime.Now
                }
            });
        }
    }
}