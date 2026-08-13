using Microsoft.CST.OAT.Utils;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.CST.OAT.Tests
{
    
    public class ReflectionHelpersTestsFixture : IDisposable
    {
        public ReflectionHelpersTestsFixture()
        {
            SetupObjects();
        }
        public const string MagicWord = "Taco";
        public const string Key = "Key";
        private void SetupObjects()
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

        internal TestObject NestedParent = new TestObject() { };

        internal TestObject NestedChild = new TestObject() { };
        public void ClassSetup()
        {
            SetupObjects();
        }

        public void Dispose()
        {
        }
    }
    
    public class ReflectionHelpersTests : IClassFixture<ReflectionHelpersTestsFixture>
    {
        private readonly ReflectionHelpersTestsFixture fixture;

        public ReflectionHelpersTests(ReflectionHelpersTestsFixture fixture)
        {
            this.fixture = fixture;
        }
        
        [Fact]
        public void TestObjDict()
        {
            Assert.Equal(ReflectionHelpersTestsFixture.MagicWord, Helpers.GetValueByPropertyOrFieldName(fixture.NestedParent, $"ObjDictField.{ReflectionHelpersTestsFixture.Key}.StringField"));
        }

        [Fact]
        public void TestStringDict()
        {
            Assert.Equal(ReflectionHelpersTestsFixture.MagicWord, Helpers.GetValueByPropertyOrFieldName(fixture.NestedParent, $"StringDictField.{ReflectionHelpersTestsFixture.Key}"));
        }

        [Fact]
        public void TestObjList()
        {
            Assert.Equal(ReflectionHelpersTestsFixture.MagicWord, Helpers.GetValueByPropertyOrFieldName(fixture.NestedParent, $"ObjListField.0.StringField"));
        }

        [Fact]
        public void TestStringList()
        {
            Assert.Equal(ReflectionHelpersTestsFixture.MagicWord, Helpers.GetValueByPropertyOrFieldName(fixture.NestedParent, $"StringListField.0"));
        }

        [Fact]
        public void TestDirectLookup()
        {
            Assert.Equal(ReflectionHelpersTestsFixture.MagicWord, Helpers.GetValueByPropertyOrFieldName(fixture.NestedParent, $"StringField"));
        }
    }
}
