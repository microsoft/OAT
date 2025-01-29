using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Microsoft.CST.OAT.VehicleDemo;
using System;

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

        public class TestClass3
        {
            public TestClass3(string _)
            {

            }
        }

        public class TestClass4
        {
            public TestClass4(TestClass tc, TestClass3 tc3)
            {

            }
        }

        public class TestClass5
        {
            public TestClass5(Vehicle v)
            {

            }
        }

        [TestMethod]
        public void TestConstructedOfLoadedTypes()
        {
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(TestClass)));
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(TestClass2)));
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(TestClass3)));
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(TestClass4)));
            Assert.IsTrue(Helpers.ConstructedOfLoadedTypes(typeof(TestClass5)));
        }
    }
}
