using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string SourcePrefix = "Trading212_";
        private readonly HttpClient _httpClient;
        private readonly IExpensesService expenses;

        public Trading212Importer(HttpClient httpClient, IExpensesService expenses)
        {
            _httpClient = httpClient;
            this.expenses = expenses;
            _httpClient.BaseAddress = new Uri("https://live.services.trading212.com");
        }
        
        public ImportResult ImportTransactions(string loginToken, string trading212SessionLive)
        {
            var added = new List<Transaction>();
            var skipped = new List<CreateTransactionResult>();
            var duplicateFound = false;
            string lastId = null;
            var totalAddedTransactions = new List<Transaction>();

            while (!duplicateFound)
            {
                var transactions = new List<Transaction>();
                try
                {
                    transactions = GetTransactionExecutions(loginToken, trading212SessionLive, lastId);
                }
                catch (Exception e)
                {
                    return new ImportResult()
                    {
                        Added = totalAddedTransactions,
                        Skipped = skipped,
                        Error = e.Message
                    };
                }

                if (transactions.Count == 0)
                    break;

                IEnumerable<CreateTransactionResult> skippedResults;
                expenses.TryCreateTransactions(transactions, out skippedResults);
                lastId = transactions[transactions.Count - 1].TransactionId;

                var addedTransactions = new List<Transaction>();
                if (skippedResults != null && skippedResults.Count() > 0)
                {
                    addedTransactions = transactions.Where(x => !skippedResults.Any(y => y.Transaction.TransactionId == x.TransactionId)).ToList();
                    skipped.AddRange(skippedResults);

                    foreach (var skippedTransaction in skippedResults)
                    {
                        if (skippedTransaction.ReasonResult == CreateTransactionResult.Reason.DuplicateEntry)
                        {
                            duplicateFound = true;
                            break;
                        }
                    }
                }
                else
                {
                    addedTransactions = transactions;
                }

                totalAddedTransactions.AddRange(addedTransactions);
            }

            return new ImportResult()
            {
                Added = totalAddedTransactions,
                Skipped = skipped
            };
        }

        private List<Transaction> GetTransactionExecutions(string loginToken, string trading212SessionLive, string earlierThanTransactionId = null)
        {
            // Base URL with mandatory query parameter
            var url = "/rest/cards/v1/transaction-executions?pageSize=50";

            // Append optional cursorId if provided
            if (earlierThanTransactionId != null)
            {
                url += $"&cursorId={earlierThanTransactionId}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add the Cookie header with the provided token values
            request.Headers.Add("Cookie",
                $"LOGIN_TOKEN={loginToken}; TRADING212_SESSION_LIVE={trading212SessionLive}");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("User-Agent", "PostmanRuntime/7.43.0");
            //request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Host", "live.services.trading212.com");

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
                if (t.Type != "PURCHASE" ||
                    (t.Status != "COMPLETED" && t.Status != "PENDING"))
                    continue;

                mapped.Add(new Transaction
                {
                    TransactionId = t.Id.ToString(),
                    Amount = t.Amount,
                    Date = t.TimeCreated,
                    Details = $"{t.Merchant.Name} {t.Merchant.Category} {t.Merchant.Address}  {t.Merchant.CountryCode}",
                    Source = SourcePrefix + t.CardLastFour,
                    Type = TransactionType.Expense
                });
            }

            return mapped;
        }        // Add a new method to process transactions directly from JSON
        public ImportResult ImportTransactionsFromJson(string json)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var transactions = Map(JsonSerializer.Deserialize<List<TransactionExecution>>(json, options));

                if (transactions.Count == 0)
                {
                    return new ImportResult
                    {
                        Added = new List<Transaction>(),
                        Skipped = new List<CreateTransactionResult>(),
                        Error = "No valid transactions found in the provided JSON"
                    };
                }

                IEnumerable<CreateTransactionResult> skipped;
                expenses.TryCreateTransactions(transactions, out skipped);

                var addedTransactions = transactions.Where(x => !skipped.Any(y => y.Transaction.TransactionId == x.TransactionId)).ToList();

                return new ImportResult
                {
                    Added = addedTransactions,
                    Skipped = skipped
                };
            }
            catch (Exception e)
            {
                return new ImportResult
                {
                    Added = new List<Transaction>(),
                    Skipped = new List<CreateTransactionResult>(),
                    Error = $"Error processing JSON: {e.Message}"
                };
            }
        }
    }
}
