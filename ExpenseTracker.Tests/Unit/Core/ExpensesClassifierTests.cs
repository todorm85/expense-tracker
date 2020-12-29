using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ExpenseTracker.Allianz.Tests
{
    [TestClass]
    public class ExpensesClassifierTests
    {
        private List<Category> categories;
        private Transaction expense;
        private TransactionsClassifier sut;

        [TestMethod]
        public void Classify_CategoriesThatDoNotMatch()
        {
            this.expense.Details = "trop dedov opa";
            this.expense.Category = "test";
            this.categories.Add(new Category() { KeyWord = "pisana", Name = "cat1" });
            this.sut.Classify(new Transaction[] { this.expense }, this.categories);
            Assert.AreEqual("test", this.expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatch()
        {
            this.expense.Details = "trop dedov opa";
            this.categories.Add(new Category() { KeyWord = "dedov", Name = "cat1" });
            this.sut.Classify(new Transaction[] { this.expense }, this.categories);
            Assert.AreEqual("cat1", this.expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatchAndDoNotMatch()
        {
            this.expense.Details = "trop pisana opa";
            this.categories.Add(new Category() { KeyWord = "dedov", Name = "cat1" });
            this.categories.Add(new Category() { KeyWord = "pisana", Name = "cat2" });
            this.sut.Classify(new Transaction[] { this.expense }, this.categories);
            Assert.AreEqual("cat2", this.expense.Category);
        }

        [TestMethod]
        public void Classify_ExpensesWithNullSource()
        {
            this.expense.Category = "test";
            this.categories.Add(new Category() { KeyWord = "pisana", Name = "cat1" });
            this.sut.Classify(new Transaction[] { this.expense }, this.categories);
            Assert.AreEqual("test", this.expense.Category);
        }

        [TestMethod]
        public void Classify_NoCategories_NoClassification()
        {
            this.expense.Details = "trop dedov opa";
            this.sut.Classify(new Transaction[] { this.expense }, this.categories);
            Assert.IsNull(this.expense.Category);
        }

        [TestInitialize]
        public void Setup()
        {
            this.expense = new Transaction();
            this.categories = new List<Category>();
            this.sut = new TransactionsClassifier();
        }
    }
}