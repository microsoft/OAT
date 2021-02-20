using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CST.OAT.VehicleDemo;
using System.Reflection;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class HelpersTest
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        [TestMethod]
        public static void TestConstructedOfLoadedTypes()
        {
            Assert.IsFalse(Helpers.ConstructedOfLoadedTypes(typeof(Vehicle)));
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(Vehicle), new Assembly[] { typeof(Vehicle).Assembly }));
        }
    }
}
