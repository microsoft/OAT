using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class ReflectionHelpersTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
            SetupObjects();
        }

        private const string MagicWord = "Taco";
        private const string Key = "Key";

        private static void SetupObjects()
        {
            NestedChild = new TestObject()
            {
                StringField = MagicWord
            };
            NestedParent = new TestObject()
            {
                ObjDictField = new Dictionary<string, object?>()
                {
                    { Key, NestedChild }
                },
                ObjListField = new List<object?>() { NestedChild },
                StringDictField = new Dictionary<string, string>()
                {
                    { Key, MagicWord }
                },
                StringListField = new List<string>()
                {
                    MagicWord
                },
                StringField = MagicWord
            };
        }

        private static TestObject NestedParent = new TestObject() { };

        private static TestObject NestedChild = new TestObject() { };

        [TestMethod]
        public void TestObjDict()
        {
            Assert.AreEqual(MagicWord, Helpers.GetValueByPropertyOrFieldName(NestedParent, $"ObjDictField.{Key}.StringField"));
        }

        [TestMethod]
        public void TestStringDict()
        {
            Assert.AreEqual(MagicWord, Helpers.GetValueByPropertyOrFieldName(NestedParent, $"StringDictField.{Key}"));
        }

        [TestMethod]
        public void TestObjList()
        {
            Assert.AreEqual(MagicWord, Helpers.GetValueByPropertyOrFieldName(NestedParent, $"ObjListField.0.StringField"));
        }

        [TestMethod]
        public void TestStringList()
        {
            Assert.AreEqual(MagicWord, Helpers.GetValueByPropertyOrFieldName(NestedParent, $"StringListField.0"));
        }

        [TestMethod]
        public void TestDirectLookup()
        {
            Assert.AreEqual(MagicWord, Helpers.GetValueByPropertyOrFieldName(NestedParent, $"StringField"));
        }
    }
}
