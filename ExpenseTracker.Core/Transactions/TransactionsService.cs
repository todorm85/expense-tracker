using ExpenseTracker.Core.Categories;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExpenseTracker.Core.Transactions
{
    public class TransactionsService : BaseDataItemService<Transaction>, ITransactionsService
    {
        private readonly IUnitOfWork data;
        private readonly IBaseDataItemService<Category> categoriesRepo;
        private readonly TransactionsClassifier classifier = new TransactionsClassifier();
        private IBaseDataItemService<Rule> rulesService;

        public TransactionsService(IUnitOfWork data, IBaseDataItemService<Category> categoriesRepo, IBaseDataItemService<Rule> rulesService) : base(data)
        {
            this.data = data;
            this.categoriesRepo = categoriesRepo;
            this.rulesService = rulesService;
        }

        public static string GenerateTransactionId(DateTime date, decimal amount, string details)
        {
            var detailsHash = details.ComputeCRC32Hash();
            return $"{date.ToString("dd.MM.yy.HH.mm.ss", CultureInfo.InvariantCulture)}_{amount.ToString("F2")}_{detailsHash}";
        }

        public List<List<Transaction>> GetPotentialDuplicates()
        {
            var orderedTransactions = this.repo.GetAll().OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Amount)
                .ThenByDescending(x => x.Details);
            List<List<Transaction>> result = new List<List<Transaction>>();
            Transaction lastTransaction = null;
            var currentBatch = new List<Transaction>();
            foreach (var t in orderedTransactions)
            {
                if (lastTransaction == null)
                {
                    lastTransaction = t;
                    continue;
                }

                if (lastTransaction.Date.Year == t.Date.Year
                    && lastTransaction.Date.Month == t.Date.Month
                    && lastTransaction.Date.Day == t.Date.Day
                    && lastTransaction.Amount == t.Amount)
                {
                    currentBatch.Add(t);
                }
                else
                {
                    lastTransaction = t;
                    if (currentBatch.Count > 1 && currentBatch.Any(x => !x.IsResolvedDuplicate))
                    {
                        currentBatch.ForEach(x => x.IsResolvedDuplicate = false);
                        this.Update(currentBatch);
                        result.Add(currentBatch);
                    }

                    currentBatch = new List<Transaction>();
                    currentBatch.Add(t);
                }
            }

            return result;
        }

        public void Import(decimal amount, string details, DateTime date, TransactionType type, string id = null)
        {
            if (date == default(DateTime))
                throw new InvalidDataException("Cannot persist transaction without proper date set.");
            if (amount < 0)
                throw new InvalidDataException("Cannot persist transaction with negative amount. Use transaction type to specify either debit or credit.");
            if (type == TransactionType.Unspecified)
                throw new InvalidOperationException("Cannot persist transaction with unspecified type. Either debit or credit must be set.");

            var t = new Transaction()
            {
                TransactionId = id ?? GenerateTransactionId(date, amount, details),
                Amount = amount,
                Details = details,
                Type = type,
                Date = date
            };

            this.classifier.Classify(new Transaction[] { t }, this.categoriesRepo.GetAll());
            this.Add(t);
        }

        public bool TryAdd(Transaction t, out IEnumerable<TransactionInsertResult> skipped)
        {
            return this.TryAdd(new Transaction[] { t }, out skipped);
        }

        public bool TryAdd(IEnumerable<Transaction> expenses, out IEnumerable<TransactionInsertResult> skipped)
        {
            var toAdd = new List<Transaction>();
            var cats = this.categoriesRepo.GetAll();
            var skippedResult = new List<TransactionInsertResult>();
            var rules = this.rulesService.GetAll();
            foreach (var t in expenses)
            {
                var skip = false;
                foreach (Rule rule in rules)
                {
                    if (!rule.Process(t))
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.Skipped));
                    continue;
                }

                if (t.Date == default(DateTime))
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.InvalidDate));
                    continue;
                }
                if (t.Amount < 0)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.InvalidAmount));
                    continue;
                }
                if (t.Type == TransactionType.Unspecified)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.InvalidType));
                    continue;
                }

                var transactionId = string.IsNullOrEmpty(t.TransactionId) ? GenerateTransactionId(t.Date, t.Amount, t.Details) : t.TransactionId;
                if (this.GetById(transactionId) != null)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.DuplicateEntry));
                    continue;
                }

                t.TransactionId = transactionId;
                this.classifier.Classify(new Transaction[] { t }, cats);                

                toAdd.Add(t);
            }

            this.repo.Insert(toAdd);
            skipped = skippedResult;
            return skipped.Count() == 0;
        }

        public override void Add(IEnumerable<Transaction> items)
        {
            this.TryAdd(items, out IEnumerable<TransactionInsertResult> skipped);
            if (skipped.Count() > 0)
            {
                throw new ArgumentException("Some of the transactions were not added due to errors. Please use TryAdd to inspect the skipped items without throwing error.");
            }
        }

        public override void Add(Transaction item)
        {
            this.Add(new Transaction[] { item });
        }
    }

    public class TransactionInsertResult
    {
        public TransactionInsertResult(Transaction t, Reason r)
        {
            this.Transaction = t;
            this.ReasonResult = r;
        }

        public Transaction Transaction { get; set; }
        public Reason ReasonResult { get; set; }

        public enum Reason
        {
            InvalidDate,
            InvalidAmount,
            DuplicateEntry,
            InvalidType,
            Skipped
        }
    }

}