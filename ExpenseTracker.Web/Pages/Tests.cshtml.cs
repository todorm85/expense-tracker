using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages
{
    public class TestsModel : PageModel
    {
        private readonly ExpensesSeeder seeder;

        public TestsModel(IRepository<Transaction> transactions)
        {
            this.seeder = new ExpensesSeeder(transactions);
        }

        public void OnGet()
        {
        }

        public void OnPostSeedData()
        {
            seeder.Seed();
        }
    }

    public class ExpensesSeeder
    {
        private readonly IRepository<Transaction> transactionsRepo;
        private static readonly Random random = new Random();

        public ExpensesSeeder(IRepository<Transaction> transactionsRepo)
        {
            this.transactionsRepo = transactionsRepo;
        }

        public void Seed()
        {
            var transactions = new List<Transaction>();
            var startDate = new DateTime(2023, 12, 1);
            var endDate = new DateTime(2024, 12, 31);
            var categories = new[] { "food", "others", "car" };
            var detailsOptions = new[] { "Sample details", "Random details", "Test details", "Expense details" };

            for (var date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                for (int i = 0; i < 15; i++)
                {
                    var transaction = new Transaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        Date = date.AddDays(random.Next(0, DateTime.DaysInMonth(date.Year, date.Month))),
                        Amount = random.Next(1, 1000),
                        Details = detailsOptions[random.Next(detailsOptions.Length)],
                        Type = TransactionType.Expense,
                        Source = "Seeder"
                    };

                    if (random.NextDouble() <= 0.9)
                    {
                        var categoryCount = random.Next(1, 3);
                        var assignedCategories = new List<string>();
                        for (int j = 0; j < categoryCount; j++)
                        {
                            assignedCategories.Add(categories[random.Next(categories.Length)]);
                        }
                        transaction.Category = string.Join(" ", assignedCategories);
                    }

                    transactions.Add(transaction);
                }
            }

            transactionsRepo.Insert(transactions);
        }
    }
}
