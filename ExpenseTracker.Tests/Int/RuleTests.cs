using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace ExpenseTracker.Tests.Int
{
    [TestClass]
    public class RuleTests : IntTestsBase
    {
        private IBaseDataItemService<Rule> rules;
        private TransactionsService expenses;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.rules = container.Resolve<IBaseDataItemService<Rule>>();
            this.expenses = container.Resolve<TransactionsService>();
        }

        [TestMethod]
        public void SkipTransactionWorksCorrectly()
        {
            var t = new Transaction()
            {
                Details = "Some test text.",
                Type = TransactionType.Expense,
                Date = DateTime.Now,
                TransactionId = "id1"
            };

            this.rules.Add(new Rule()
            {
                Action = RuleAction.Skip,
                Condition = RuleCondition.Contains,
                Property = "Details",
                Value = " text."
            });

            this.expenses.TryAdd(t, out IEnumerable<TransactionInsertResult> skipped);
            var skip = skipped.FirstOrDefault();
            Assert.AreNotEqual(null, skip);
            Assert.AreEqual(TransactionInsertResult.Reason.Skipped, skip.ReasonResult);

            var added = this.expenses.GetAll();
            Assert.AreEqual(0, added.Count());

            t.TransactionId = "id2";
            t.Details = "other text";
            this.expenses.TryAdd(t, out IEnumerable<TransactionInsertResult> skipped2);
            skip = skipped2.FirstOrDefault();
            Assert.AreEqual(null, skip);

            added = this.expenses.GetAll();
            Assert.AreEqual(1, added.Count());
        }

        [TestMethod]
        public void SetPropertyWorksCorrectly()
        {
            var t = new Transaction()
            {
                Details = "Some test text.",
                Date = DateTime.Now,
                TransactionId = "id1"
            };

            this.rules.Add(new Rule()
            {
                Action = RuleAction.SetProperty,
                Condition = RuleCondition.Contains,
                Property = "Details",
                Value = " text.",
                PropertyToSet = "Type",
                ValueToSet = TransactionType.Income.ToString()
            });

            this.expenses.TryAdd(t, out IEnumerable<TransactionInsertResult> skipped);
            var skip = skipped.FirstOrDefault();
            Assert.AreEqual(null, skip);

            var added = this.expenses.GetAll();
            Assert.AreEqual(1, added.Count());
            Assert.AreEqual(TransactionType.Income, added.First().Type);
        }
    }
}
