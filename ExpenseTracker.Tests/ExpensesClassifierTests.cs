using System;
using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesClassifierTests
    {
        private ExpensesClassifier sut;
        private Expense expense;

        [TestInitialize]
        public void Setup()
        {
            this.expense = new Expense();
            this.sut = new ExpensesClassifier(null);
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
            sut.KeysCategories.Add("dedov", "cat1");
            sut.Classify(new Expense[] { expense });
            Assert.AreEqual("cat1", expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatDoNotMatch()
        {
            expense.Source = "trop dedov opa";
            sut.KeysCategories.Add("pisana", "cat1");
            sut.Classify(new Expense[] { expense });
            Assert.IsNull(expense.Category);
        }

        [TestMethod]
        public void Classify_CategoriesThatMatchAndDoNotMatch()
        {
            expense.Source = "trop pisana opa";
            sut.KeysCategories.Add("dedov", "cat1");
            sut.KeysCategories.Add("pisana", "cat2");
            sut.Classify(new Expense[] { expense });
            Assert.AreEqual("cat2", expense.Category);
        }
    }
}
