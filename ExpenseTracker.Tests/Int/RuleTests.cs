using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;

namespace ExpenseTracker.Tests.Int
{
    [TestClass]
    public class RuleTests : IntTestsBase
    {
        private IReadRepository<Transaction> TransactionsRepo => expenses as IReadRepository<Transaction>;
        private IRepository<Rule> rules;
        private ExpensesService expenses;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.rules = container.Resolve<IRepository<Rule>>();
            this.expenses = container.Resolve<ExpensesService>();
        }

        [TestMethod]
        public void SkipTransactionWorksCorrectly()
        {
            var t = new Transaction()
            {
                Details = "Some test text.",
                Type = TransactionType.Expense,
                Date = DateTime.Now,
                TransactionId = "id1",
                Source = "tests",
                Amount = 1
            };

            this.rules.Insert(new Rule()
            {
                Action = RuleAction.Skip,
                Condition = RuleCondition.Contains,
                Property = "Details",
                ConditionValue = " text."
            });

            this.expenses.TryCreateTransaction(t, out IEnumerable<CreateTransactionResult> skipped);
            var skip = skipped.FirstOrDefault();
            Assert.AreNotEqual(null, skip);
            Assert.AreEqual(CreateTransactionResult.Reason.Skipped, skip.ReasonResult);

            var added = this.TransactionsRepo.GetAll();
            Assert.AreEqual(0, added.Count());

            t.TransactionId = "id2";
            t.Details = "other text";
            this.expenses.TryCreateTransaction(t, out IEnumerable<CreateTransactionResult> skipped2);
            skip = skipped2.FirstOrDefault();
            Assert.AreEqual(null, skip);

            added = this.TransactionsRepo.GetAll();
            Assert.AreEqual(1, added.Count());
        }

        [TestMethod]
        public void SetPropertyWorksCorrectly()
        {
            var t = new Transaction()
            {                
                Details = "Some test text.",
                Date = DateTime.Now,
                TransactionId = "id1",
                Source = "tests",
                Amount = 1
            };

            this.rules.Insert(new Rule()
            {
                Action = RuleAction.SetProperty,
                Condition = RuleCondition.Contains,
                Property = "Details",
                ConditionValue = " text.",
                PropertyToSet = "Type",
                ValueToSet = TransactionType.Income.ToString()
            });

            this.expenses.TryCreateTransaction(t, out IEnumerable<CreateTransactionResult> skipped);
            var skip = skipped.FirstOrDefault();
            Assert.AreEqual(null, skip);

            var added = this.TransactionsRepo.GetAll();
            Assert.AreEqual(1, added.Count());
            Assert.AreEqual(TransactionType.Income, added.First().Type);
        }
    }
}
