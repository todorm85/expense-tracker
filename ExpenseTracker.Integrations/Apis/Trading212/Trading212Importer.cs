using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.ApiClients.Trading212
{
    public class Trading212Importer
    {
        private readonly HttpClient _httpClient;
        private readonly IExpensesService expenses;

        public Trading212Importer(HttpClient httpClient, IExpensesService expenses)
        {
            _httpClient = httpClient;
            this.expenses = expenses;
            _httpClient.BaseAddress = new Uri("https://live.services.trading212.com");
        }

        public void ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<CreateTransactionResult> skipped)
        {
            expenses.TryCreateTransactions(GetTransactionExecutions(), out skipped);
        }

        private List<Transaction> GetTransactionExecutions(string loginToken, string trading212SessionLive, long? earlierThanTransactionId = null)
        {
            // Base URL with mandatory query parameter
            var url = "/rest/cards/v1/transaction-executions?pageSize=50";

            // Append optional cursorId if provided
            if (earlierThanTransactionId.HasValue)
            {
                url += $"&cursorId={earlierThanTransactionId.Value}"; 
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add the Cookie header with the provided token values
            request.Headers.Add("Cookie",
                $"LOGIN_TOKEN={loginToken}; TRADING212_SESSION_LIVE={trading212SessionLive}");

            // Blocking call to send the request
            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return Map(JsonSerializer.Deserialize<List<TransactionExecution>>(jsonResponse, options));
        }

        private List<Transaction> Map(IList<TransactionExecution> transactions)
        {
            var mapped = new List<Transaction>();
            foreach (var t in transactions)
            {
                if (t.Type != "PURCHASE")
                    continue;
                
                mapped.Add(new Transaction
                {
                    TransactionId = t.Id.ToString(),
                    Amount = t.Amount,
                    Date = t.TimeCreated,
                    Details = $"{t.Merchant.Name} {t.Merchant.Category} {t.Merchant.Address}  {t.Merchant.CountryCode}",
                    Source = "Trading212_" + t.CardLastFour,
                    Type = TransactionType.Expense
                });
            }

            return mapped;
        }
    }
}
