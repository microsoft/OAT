using System;
using System.Collections.Generic;

namespace Microsoft.CST.OAT.Tests
{
    class TestObject
    {
        public int IntField { get; set; }
        public string? StringField { get; set; }
        public bool BoolField { get; set; }
        public Dictionary<string, string>? StringDictField { get; set; }
        public List<string>? StringListField { get; set; }
        public TestObject? NestedObject { get; set; }
        public Dictionary<string, List<string>>? StringListDictField { get; set; }
        public DateTime DateTimeField { get; set; }
    }
}
