using System;
using System.Collections.Generic;
using System.Text;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.App;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Injection;

namespace ExpenseTracker.Tests.Int
{
    [TestClass]
    public class MailImporterTests : IntTestsBase
    {
        private MailImporter sut;

        [TestInitialize]
        public void TestInit()
        {

            this.sut = container.Resolve<MailImporter>();
        }

        [TestMethod]
        public void ImportsAllianzAndReiffeizen()
        {

        }
    }
}
