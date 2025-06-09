using Microsoft.CST.OAT.Utils;
using Microsoft.CST.OAT.VehicleDemo;
using Xunit;

namespace Microsoft.CST.OAT.Tests
{
    
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

        [Fact]
        public void TestConstructedOfLoadedTypes()
        {
            Assert.True(Helpers.ConstructedOfLoadedTypes(typeof(TestClass)));
            Assert.True(Helpers.ConstructedOfLoadedTypes(typeof(TestClass2)));
            Assert.True(Helpers.ConstructedOfLoadedTypes(typeof(TestClass3)));
            Assert.True(Helpers.ConstructedOfLoadedTypes(typeof(TestClass4)));
            Assert.True(Helpers.ConstructedOfLoadedTypes(typeof(TestClass5)));
        }
    }
}
