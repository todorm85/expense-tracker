using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Rules;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Core.Services
{
    public class ExpensesService : IExpensesService
    {
        private readonly IRepository<Transaction> transactionsRepo;
        private IRepository<Rule> rulesRepo;

        public ExpensesService(IRepository<Transaction> transactionsRepo, IRepository<Rule> rulesRepo)
        {
            this.transactionsRepo = transactionsRepo;
            this.rulesRepo = rulesRepo;
        }

        public static string GenerateTransactionId(DateTime date, decimal amount, string details)
        {
            var detailsHash = details.ComputeShortHash();
            return $"{date.ToString("dd.MM.yy.HH.mm.ss", CultureInfo.InvariantCulture)}_{amount.ToString("F2")}_{detailsHash}";
        }

        #region Transactions CRUD

        public int Count(Expression<Func<Transaction, bool>> expression = null)
        {
            return transactionsRepo.Count(expression);
        }

        public IEnumerable<Transaction> GetAll(
            Expression<Func<Transaction, bool>> filter = null, 
            int skip = 0, 
            int limit = int.MaxValue,
            Expression<Func<Transaction, object>> orderBy = null,
            bool ascending = true)
        {
            return this.transactionsRepo.GetAll(filter, skip, limit, orderBy, ascending);
        }

        public bool TryCreateTransaction(Transaction t, out IEnumerable<CreateTransactionResult> skipped)
        {
            return TryCreateTransactions(new Transaction[] { t }, out skipped);
        }

        public bool TryCreateTransactions(IEnumerable<Transaction> expenses, out IEnumerable<CreateTransactionResult> skipped)
        {
            if (expenses.OrderBy(x => x.TransactionId).Select(x => x.TransactionId).Distinct().Count() != expenses.Count())
            {
                var firstDuplicateKey = expenses.GroupBy(x => x.TransactionId).Where(x => x.Count() > 1).First().Key;
                throw new InvalidOperationException($"Error in file. There are elements with duplicate ids. First one is {firstDuplicateKey}");
            }

            ApplyRules(expenses, out List<Transaction> toAdd, out IList<CreateTransactionResult> toSkip);

            foreach (var t in toAdd)
            {
                if (string.IsNullOrWhiteSpace(t.TransactionId))
                {
                    toSkip.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.InvalidId));
                    continue;
                }

                if (t.Date == default)
                {
                    toSkip.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.InvalidDate));
                    continue;
                }

                if (t.Amount <= 0)
                {
                    toSkip.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.InvalidAmount));
                    continue;
                }

                if (t.Type == TransactionType.Unspecified)
                {
                    toSkip.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.InvalidType));
                    continue;
                }

                if (string.IsNullOrEmpty(t.Source))
                {
                    toSkip.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.InvalidSource));
                    continue;
                }

                if (transactionsRepo.GetById(t.TransactionId) != null)
                {
                    toSkip.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.DuplicateEntry));
                    continue;
                }
            }

            skipped = toSkip;
            transactionsRepo.Insert(toAdd.Except(toSkip.Select(x => x.Transaction)));
            return skipped.Count() == 0;
        }

        public void UpdateTransactions(IEnumerable<Transaction> items)
        {
            foreach (var c in items)
            {
                UpdateTransaction(c);
            }
        }

        public void UpdateTransaction(Transaction t)
        {
            var oldValue = transactionsRepo.GetById(t.TransactionId);
            if (oldValue != null && t.Category != oldValue.Category)
            {
                var newRule = ProcessCategoryName(t);
                if (newRule != null)
                {
                    CreateRule(newRule);
                }

                categoriesCache.Clear();
            }

            transactionsRepo.Update(t);
        }

        public void RemoveTransaction(string id)
        {
            categoriesCache.Clear();
            transactionsRepo.RemoveById(id);
        }

        #endregion

        #region Rules crud

        public IEnumerable<Rule> GetAll(
            Expression<Func<Rule, bool>> filter = null, 
            int skip = 0, 
            int limit = int.MaxValue,
            Expression<Func<Rule, object>> orderBy = null,
            bool ascending = true)
        {
            return this.rulesRepo.GetAll(filter, skip, limit, orderBy, ascending);
        }

        public int Count(Expression<Func<Rule, bool>> filter = null)
        {
            return rulesRepo.Count(filter);
        }

        public void CreateRule(Rule createRuleModel)
        {
            rulesRepo.Insert(createRuleModel);
        }

        public void UpdateRule(Rule model)
        {
            rulesRepo.Update(model);
        }

        public void RemoveRule(int id)
        {
            this.rulesRepo.RemoveById(id);
        }

        #endregion

        public List<List<Transaction>> GetPotentialDuplicates()
        {
            var orderedTransactions = transactionsRepo.GetAll().OrderByDescending(x => x.Date)
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
                        UpdateTransactions(currentBatch);
                        result.Add(currentBatch);
                    }

                    currentBatch = new List<Transaction>();
                    currentBatch.Add(t);
                }
            }

            return result;
        }

        public void RenameCategory(string oldName, string newName)
        {
            var items = transactionsRepo.GetAll(x => x.Category == oldName).ToList();
            foreach (var item in items)
            {
                item.Category = newName;
            }

            UpdateTransactions(items);

            var rules = rulesRepo.GetAll(x => x.ValueToSet == oldName).ToList();
            foreach (var rule in rules)
            {
                rule.ValueToSet = newName;
            }

            rulesRepo.Update(rules);
        }

        public IEnumerable<string> GetAllCategories()
        {
            if (categoriesCache.Count == 0)
            {
                foreach (string c in transactionsRepo.GetAll(x => !string.IsNullOrEmpty(x.Category))
                                   .Select(x => x.Category)
                                   .OrderBy(x => x)
                                   .Distinct())
                {
                    categoriesCache.Add(c, null);
                }
            }

            return categoriesCache.Keys;
        }

        public void ProcessAllUncategorizedTransactions()
        {
            var uncategorized = transactionsRepo.GetAll(x => string.IsNullOrEmpty(x.Category));
            ApplyRules(uncategorized, out List<Transaction> processed, out IList<CreateTransactionResult> skipped);
            foreach (var item in processed.Where(x => !string.IsNullOrEmpty(x.Category)))
            {
                UpdateTransaction(item);
            }

            foreach (var t in skipped.Where(x => x.ReasonResult == CreateTransactionResult.Reason.Skipped))
            {
                t.Transaction.Category = Constants.IgnoredCategory;
                UpdateTransaction(t.Transaction);
            }
        }

        private void ApplyRules(IEnumerable<Transaction> all, out List<Transaction> added, out IList<CreateTransactionResult> skipped)
        {
            added = new List<Transaction>();
            skipped = new List<CreateTransactionResult>();

            // most specific rules should process last
            var rules = rulesRepo.GetAll().OrderBy(x => x.ConditionValue.Length);
            foreach (var t in all)
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
                    skipped.Add(new CreateTransactionResult(t, CreateTransactionResult.Reason.Skipped));
                    continue;
                }

                if (t.Category != null && !categoriesCache.ContainsKey(t.Category))
                {
                    categoriesCache.TryAdd(t.Category, null);
                }

                added.Add(t);
            }
        }

        private Rule ProcessCategoryName(Transaction t)
        {
            if (!string.IsNullOrEmpty(t.Category))
            {
                if (t.Category.Contains(":"))
                {
                    var parts = t.Category.Split(":");
                    t.Category = parts[0];
                    return new Rule() { ValueToSet = parts[0], ConditionValue = parts[1], Condition = RuleCondition.Contains, Action = RuleAction.SetProperty, Property = "Details", PropertyToSet = "Category" };
                }
            }

            return null;
        }

        private static IDictionary<string, string> categoriesCache = new Dictionary<string, string>();
    }
}