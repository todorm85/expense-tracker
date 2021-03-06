﻿using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExpenseTracker.Core.Transactions
{
    public class TransactionInsertResult
    {
        public TransactionInsertResult(Transaction t, Reason r)
        {
            this.Transaction = t;
            this.ReasonResult = r;
        }

        public enum Reason
        {
            None,
            InvalidDate,
            InvalidAmount,
            DuplicateEntry,
            InvalidType,
            Skipped,
            InvalidSource
        }

        public Reason ReasonResult { get; set; }
        public Transaction Transaction { get; set; }
    }

    public class TransactionsService : BaseDataItemService<Transaction>, ITransactionsService
    {
        private IGenericRepository<Rule> rulesService;

        public TransactionsService(IUnitOfWork data, IGenericRepository<Rule> rulesService) : base(data)
        {
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

        public override void Insert(IEnumerable<Transaction> items)
        {
            this.TryAdd(items, out IEnumerable<TransactionInsertResult> skipped);
            if (skipped.Count() > 0)
            {
                throw new ArgumentException("Some of the transactions were not added due to errors. Please use TryAdd to inspect the skipped items without throwing error.");
            }
        }

        public override void Insert(Transaction item)
        {
            this.Insert(new Transaction[] { item });
        }

        public bool TryAdd(Transaction t, out IEnumerable<TransactionInsertResult> skipped)
        {
            return this.TryAdd(new Transaction[] { t }, out skipped);
        }

        public bool TryAdd(IEnumerable<Transaction> expenses, out IEnumerable<TransactionInsertResult> skipped)
        {
            var toAdd = new List<Transaction>();
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
                if (t.Amount <= 0)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.InvalidAmount));
                    continue;
                }
                if (t.Type == TransactionType.Unspecified)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.InvalidType));
                    continue;
                }
                if (string.IsNullOrEmpty(t.Source))
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.InvalidSource));
                    continue;
                }

                var transactionId = string.IsNullOrEmpty(t.TransactionId) ? GenerateTransactionId(t.Date, t.Amount, t.Details) : t.TransactionId;
                if (this.GetById(transactionId) != null || toAdd.FirstOrDefault(x => x.TransactionId == transactionId) != null)
                {
                    skippedResult.Add(new TransactionInsertResult(t, TransactionInsertResult.Reason.DuplicateEntry));
                    continue;
                }

                t.TransactionId = transactionId;
                toAdd.Add(t);
            }

            this.repo.Insert(toAdd);
            skipped = skippedResult;
            return skipped.Count() == 0;
        }
    }
}