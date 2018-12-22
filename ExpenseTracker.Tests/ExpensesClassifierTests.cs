using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesClassifierTests
    {
        private ExpensesClassifier sut;
        private Expense expense;
        private List<Category> categories;

        [TestInitialize]
        public void Setup()
        {
            this.expense = new Expense();
            this.categories = new List<Category>();
            this.sut = new ExpensesClassifier(this.categories);
        }

        [TestMethod]
        public void Classify_NoCategories_NoClassification()
        {
            expense.Source = "trop dedov opa";
            sut.Classify(new Expense[] { expense });
            Assert.IsNull(expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatch()
        {
            expense.Source = "trop dedov opa";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "dedov", Name = "cat1" });
            sut.Classify(new Expense[] { expense });
            Assert.AreEqual("cat1", expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatDoNotMatch()
        {
            expense.Source = "trop dedov opa";
            expense.Category = "test";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "pisana", Name = "cat1" });
            sut.Classify(new Expense[] { expense });
            Assert.AreEqual("test", expense.Category);
        }

        [TestMethod]
        public void Classify_ExpensesWithNullSource()
        {
            expense.Category = "test";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "pisana", Name = "cat1" });
            sut.Classify(new Expense[] { expense });
            Assert.AreEqual("test", expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatchAndDoNotMatch()
        {
            expense.Source = "trop pisana opa";
            this.categories.Add(new Category() { ExpenseSourcePhrase = "dedov", Name = "cat1" });
            this.categories.Add(new Category() { ExpenseSourcePhrase = "pisana", Name = "cat2" });
            sut.Classify(new Expense[] { expense });
            Assert.AreEqual("cat2", expense.Category);
        }
    }
}
