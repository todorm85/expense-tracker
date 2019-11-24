using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesClassifierTests
    {
        private TransactionsClassifier sut;
        private Transaction expense;
        private List<Category> categories;

        [TestInitialize]
        public void Setup()
        {
            this.expense = new Transaction();
            this.categories = new List<Category>();
            this.sut = new TransactionsClassifier(this.categories);
        }

        [TestMethod]
        public void Classify_NoCategories_NoClassification()
        {
            expense.Details = "trop dedov opa";
            sut.Classify(new Transaction[] { expense });
            Assert.IsNull(expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatch()
        {
            expense.Details = "trop dedov opa";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "dedov", Name = "cat1" });
            sut.Classify(new Transaction[] { expense });
            Assert.AreEqual("cat1", expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatDoNotMatch()
        {
            expense.Details = "trop dedov opa";
            expense.Category = "test";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "pisana", Name = "cat1" });
            sut.Classify(new Transaction[] { expense });
            Assert.AreEqual("test", expense.Category);
        }

        [TestMethod]
        public void Classify_ExpensesWithNullSource()
        {
            expense.Category = "test";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "pisana", Name = "cat1" });
            sut.Classify(new Transaction[] { expense });
            Assert.AreEqual("test", expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatchAndDoNotMatch()
        {
            expense.Details = "trop pisana opa";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "dedov", Name = "cat1" });
            this.categories.Add(new Category() { ExpenseSourcePhrase = "pisana", Name = "cat2" });
            sut.Classify(new Transaction[] { expense });
            Assert.AreEqual("cat2", expense.Category);
        }
    }
}
