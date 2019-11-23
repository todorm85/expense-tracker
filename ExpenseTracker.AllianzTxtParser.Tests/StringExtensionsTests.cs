using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpenseTracker.Core;

namespace ExpenseTracker.AllianzTxtParser.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void RemoveRepeatingSpaces_NoRepeatingSpaces_DoesNothing()
        {
            var s = "sdfsdf fdsfdfsdsf -dsffsd".RemoveRepeatingSpaces();
            Assert.AreEqual("sdfsdf fdsfdfsdsf -dsffsd", s);
        }

        [TestMethod]
        public void RemoveRepeatingSpaces_DoubleRepeatingSpaces_LeavesOnlyOne()
        {
            var s = "sdfsdf  fdsfdfsdsf -dsffsd".RemoveRepeatingSpaces();
            Assert.AreEqual("sdfsdf fdsfdfsdsf -dsffsd", s);
        }

        [TestMethod]
        public void RemoveRepeatingSpaces_DoubleOrMoreRepeatingSpaces_LeavesOnlyOne()
        {
            var s = "sdfsdf   fdsfdfsdsf    -dsffsd".RemoveRepeatingSpaces();
            Assert.AreEqual("sdfsdf fdsfdfsdsf -dsffsd", s);
        }

        [TestMethod]
        public void RemoveRepeatingSpaces_DoubleRepeatingSpacesInBeginning_RemovesThem()
        {
            var s = "  sdfsdf fdsfdfsdsf -dsffsd".RemoveRepeatingSpaces();
            Assert.AreEqual("sdfsdf fdsfdfsdsf -dsffsd", s);
        }

        [TestMethod]
        public void RemoveRepeatingSpaces_DoubleRepeatingSpacesInEnd_RemovesThem()
        {
            var s = "sdfsdf fdsfdfsdsf -dsffsd  ".RemoveRepeatingSpaces();
            Assert.AreEqual("sdfsdf fdsfdfsdsf -dsffsd", s);
        }

        [TestMethod]
        public void RemoveRepeatingSpaces_SpacesInStartAndInEnd_RemovesThem()
        {
            var s = "    sdfsdf fdsfdfsdsf -dsffsd   ".RemoveRepeatingSpaces();
            Assert.AreEqual("sdfsdf fdsfdfsdsf -dsffsd", s);
        }
    }
}
