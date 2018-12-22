using System;
using ExpenseTracker.ConsoleClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ClassifierSettingsParserTests
    {
        [TestMethod]
        public void Parse_TwoKeyPhrasesForOneCategoryAndOneKeyPhraseForOneCategory_ParsesCorrectly()
        {
            string json = @"{
  ""food"": [
    ""FANTASTICO"",
    ""BILLA""
  ],
  ""car"": [
    ""SHELL""
  ]
}";
            var result = new CategoriesJsonParser().Parse(json);
            Assert.AreEqual(3, result.Keys.Count);
            Assert.IsTrue(result.ContainsKey("FANTASTICO"));
            Assert.IsTrue(result.ContainsKey("BILLA"));
            Assert.IsTrue(result.ContainsKey("SHELL"));
            Assert.IsTrue(result["SHELL"] == "car");
            Assert.IsTrue(result["BILLA"] == "food");
            Assert.IsTrue(result["FANTASTICO"] == "food");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_DuplicateKeyphrases_Thorws()
        {
            string json = @"{
  ""food"": [
    ""FANTASTICO"",
    ""BILLA""
  ],
  ""car"": [
    ""BILLA"",
    ""SHELL""
  ]
}";
            var result = new CategoriesJsonParser().Parse(json);
        }
    }
}
