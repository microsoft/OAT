using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class HelpersTest
    {
        public class TestClass
        {
            public TestClass(TestClass2 tc2)
            {

            }
        }

        public class TestClass2
        {
            public TestClass2()
            {

            }
        }

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        [TestMethod]
        public void TestConstructedOfLoadedTypes()
        {
            Assert.IsFalse(Helpers.ConstructedOfLoadedTypes(typeof(TestClass)));
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(TestClass), new Assembly[] { typeof(TestClass).Assembly }));
        }
    }
}
