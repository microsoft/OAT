// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.CST.OAT.Tests
{
    
    public class OperationsTests
    {
        enum ContainsTestEnum
        {
            Magic,
            Nothing
        }

        [Fact]
        public void VerifyContainsAnyOperator()
        {
            var trueStringObject = new TestObject()
            {
                StringField = "ThisStringContainsMagic"
            };

            var falseStringObject = new TestObject()
            {
                StringField = "ThisStringDoesNot"
            };

            var stringContains = new Rule("String Contains Any Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny,"StringField")
                    {
                        Data = new List<string>()
                        {
                            "Magic",
                        }
                    }
                }
            };

            var stringAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { stringContains };

            Assert.True(stringAnalyzer.Analyze(ruleList, trueStringObject).Any());
            Assert.False(stringAnalyzer.Analyze(ruleList, falseStringObject).Any());

            Assert.True(stringAnalyzer.Analyze(ruleList, null, trueStringObject).Any());
            Assert.False(stringAnalyzer.Analyze(ruleList, null, falseStringObject).Any());

            var trueListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Magic",
                    "Three"
                }
            };

            var falseListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Two",
                    "Three"
                }
            };

            var listContains = new Rule("List Contains Any Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny,"StringListField")
                    {
                        Data = new List<string>()
                        {
                            "Magic",
                            "Abra Kadabra"
                        }
                    }
                }
            };

            var listAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listContains };

            Assert.True(listAnalyzer.Analyze(ruleList, trueListObject).Any());
            Assert.False(listAnalyzer.Analyze(ruleList, falseListObject).Any());

            Assert.True(listAnalyzer.Analyze(ruleList, null, trueListObject).Any());
            Assert.False(listAnalyzer.Analyze(ruleList, null, falseListObject).Any());

            var trueEnumListObject = new TestObject()
            {
                EnumListField = new List<Enum>()
                {
                    ContainsTestEnum.Magic
                }
            };

            var falseEnumListObject = new TestObject()
            {
                EnumListField = new List<Enum>()
                {
                    ContainsTestEnum.Nothing
                }
            };

            var enumListContains = new Rule("Enum List Contains Any Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny,"EnumListField")
                    {
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    }
                }
            };

            var enumListAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { enumListContains };

            Assert.True(enumListAnalyzer.Analyze(ruleList, trueEnumListObject).Any());
            Assert.False(enumListAnalyzer.Analyze(ruleList, falseEnumListObject).Any());

            Assert.True(enumListAnalyzer.Analyze(ruleList, null, trueEnumListObject).Any());
            Assert.False(enumListAnalyzer.Analyze(ruleList, null, falseEnumListObject).Any());
            
            var trueStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Hocus Pocus" },
                    { "Two", "Abra Kadabra" },
                    { "Magic Word", "Please" }
                }
            };

            var falseStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Magic" },
                    { "Two", "Show" },
                    { "Three", "Please" }
                }
            };

            var stringDictContains = new Rule("String Dict Contains Any Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny,"StringDictField")
                    {
                        DictData = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("Magic Word","Please")
                        }
                    }
                }
            };

            var stringDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { stringDictContains };

            Assert.True(stringDictAnalyzer.Analyze(ruleList, trueStringDictObject).Any());
            Assert.False(stringDictAnalyzer.Analyze(ruleList, falseStringDictObject).Any());

            Assert.True(stringDictAnalyzer.Analyze(ruleList, null, trueStringDictObject).Any());
            Assert.False(stringDictAnalyzer.Analyze(ruleList, null, falseStringDictObject).Any());

            var trueListDictObject = new TestObject()
            {
                StringListDictField = new Dictionary<string, List<string>>()
                {
                    {
                        "Magic Words", new List<string>()
                        {
                            "Please",
                            "Thank You"
                        }
                    }
                }
            };

            var falseListDictObject = new TestObject()
            {
                StringListDictField = new Dictionary<string, List<string>>()
                {
                    {
                        "Magic Words", new List<string>()
                        {
                            "Scallywag",
                            "Magic"
                        }
                    }
                }
            };

            var listDictContains = new Rule("List Dict Contains Any Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny,"StringListDictField")
                    {
                        DictData = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("Magic Words","Please"),
                            new KeyValuePair<string, string>("Magic Words","Thank You"),
                        }
                    }
                }
            };

            var listDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listDictContains };

            Assert.True(listDictAnalyzer.Analyze(ruleList, trueListDictObject).Any());
            Assert.False(listDictAnalyzer.Analyze(ruleList, falseListDictObject).Any());

            Assert.True(listDictAnalyzer.Analyze(ruleList, null, trueListDictObject).Any());
            Assert.False(listDictAnalyzer.Analyze(ruleList, null, falseListDictObject).Any());

            var enumFlagsContains = new Rule("Enum Flags Contains Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny)
                    {
                        Data = new List<string>(){"Magic", "Normal"}
                    }
                }
            };

            var enumAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { enumFlagsContains };

            Assert.True(enumAnalyzer.Analyze(ruleList, Words.Magic).Any());
            Assert.True(enumAnalyzer.Analyze(ruleList, Words.Normal).Any());
            Assert.True(enumAnalyzer.Analyze(ruleList, Words.Magic | Words.Normal).Any());
            Assert.False(enumAnalyzer.Analyze(ruleList, Words.Shazam).Any());

            Assert.True(enumAnalyzer.Analyze(ruleList, null, Words.Magic).Any());
            Assert.True(enumAnalyzer.Analyze(ruleList, null, Words.Normal).Any());
            Assert.True(enumAnalyzer.Analyze(ruleList, null, Words.Magic | Words.Normal).Any());
            Assert.False(enumAnalyzer.Analyze(ruleList, null, Words.Shazam).Any());
        }

        [Fact]
        public void VerifyContainsKeyOperator()
        {
            var trueAlgDict = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "Magic", "Anything" }
                }
            };

            var falseAlgDict = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "No Magic", "Anything" }
                }
            };

            var dictContainsKey = new Rule("DictContainsKey")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsKey, "StringDictField")
                    {
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    }
                }
            };

            var algDictAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { dictContainsKey };

            Assert.True(algDictAnalyzer.Analyze(ruleList, trueAlgDict).Any());
            Assert.False(algDictAnalyzer.Analyze(ruleList, falseAlgDict).Any());

            Assert.True(algDictAnalyzer.Analyze(ruleList, null, trueAlgDict).Any());
            Assert.False(algDictAnalyzer.Analyze(ruleList, null, falseAlgDict).Any());
        }

        [Fact]
        public void VerifyContainsOperator()
        {
            var trueStringObject = new TestObject()
            {
                StringField = "ThisStringContainsMagic"
            };

            var falseStringObject = new TestObject()
            {
                StringField = "ThisOneDoesNot"
            };

            var stringContains = new Rule("String Contains Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "Magic",
                            "String"
                        }
                    }
                }
            };

            var stringAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { stringContains };

            Assert.True(stringAnalyzer.Analyze(ruleList, trueStringObject).Any());
            Assert.False(stringAnalyzer.Analyze(ruleList, falseStringObject).Any());

            Assert.True(stringAnalyzer.Analyze(ruleList, null, trueStringObject).Any());
            Assert.False(stringAnalyzer.Analyze(ruleList, null, falseStringObject).Any());

            var trueListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Magic",
                    "Abra Kadabra"
                }
            };

            var falseListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Two",
                    "Three"
                }
            };

            var listContains = new Rule("List Contains Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains, "StringListField")
                    {
                        Data = new List<string>()
                        {
                            "Magic",
                            "Abra Kadabra"
                        }
                    }
                }
            };

            var listAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listContains };

            Assert.True(listAnalyzer.Analyze(ruleList, trueListObject).Any());
            Assert.False(listAnalyzer.Analyze(ruleList, falseListObject).Any());

            Assert.True(listAnalyzer.Analyze(ruleList, null, trueListObject).Any());
            Assert.False(listAnalyzer.Analyze(ruleList, null, falseListObject).Any());

            var trueEnumListObject = new TestObject()
            {
                EnumListField = new List<Enum>()
                {
                    ContainsTestEnum.Magic
                }
            };

            var falseEnumListObject = new TestObject()
            {
                EnumListField = new List<Enum>()
                {
                    ContainsTestEnum.Nothing
                }
            };

            var enumListContains = new Rule("Enum List Contains Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains,"EnumListField")
                    {
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    }
                }
            };

            var enumListAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { enumListContains };

            Assert.True(enumListAnalyzer.Analyze(ruleList, trueEnumListObject).Any());
            Assert.False(enumListAnalyzer.Analyze(ruleList, falseEnumListObject).Any());

            Assert.True(enumListAnalyzer.Analyze(ruleList, null, trueEnumListObject).Any());
            Assert.False(enumListAnalyzer.Analyze(ruleList, null, falseEnumListObject).Any());
            
            var trueStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Money" },
                    { "Two", "Show" },
                    { "Magic Word", "Please" }
                }
            };

            var falseStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Magic" },
                    { "Two", "Something" },
                    { "Three", "Please" }
                }
            };

            var stringDictContains = new Rule("String Dict Contains Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains, "StringDictField")
                    {
                        DictData = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("Magic Word","Please"),
                            new KeyValuePair<string, string>("Two","Show"),
                            new KeyValuePair<string, string>("One","Money")
                        }
                    }
                }
            };

            var stringDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { stringDictContains };

            Assert.True(stringDictAnalyzer.Analyze(ruleList, trueStringDictObject).Any());
            Assert.False(stringDictAnalyzer.Analyze(ruleList, falseStringDictObject).Any());

            Assert.True(stringDictAnalyzer.Analyze(ruleList, null, trueStringDictObject).Any());
            Assert.False(stringDictAnalyzer.Analyze(ruleList, null, falseStringDictObject).Any());

            var trueListDictObject = new TestObject()
            {
                StringListDictField = new Dictionary<string, List<string>>()
                {
                    {
                        "Magic Words", new List<string>()
                        {
                            "Please",
                            "Thank You"
                        }
                    }
                }
            };

            var falseListDictObject = new TestObject()
            {
                StringListDictField = new Dictionary<string, List<string>>()
                {
                    {
                        "Magic Words", new List<string>()
                        {
                            "Scallywag",
                            "Magic"
                        }
                    }
                }
            };

            var listDictContains = new Rule("List Dict Contains Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains, "StringListDictField")
                    {
                        DictData = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("Magic Words","Please"),
                            new KeyValuePair<string, string>("Magic Words","Thank You"),
                        }
                    }
                }
            };

            var listDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listDictContains };

            Assert.True(listDictAnalyzer.Analyze(ruleList, trueListDictObject).Any());
            Assert.False(listDictAnalyzer.Analyze(ruleList, falseListDictObject).Any());

            Assert.True(listDictAnalyzer.Analyze(ruleList, null, trueListDictObject).Any());
            Assert.False(listDictAnalyzer.Analyze(ruleList, null, falseListDictObject).Any());

            var enumFlagsContains = new Rule("Enum Flags Contains Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains)
                    {
                        Data = new List<string>(){"Magic", "Normal"}
                    }
                }
            };

            var enumAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { enumFlagsContains };

            Assert.True(enumAnalyzer.Analyze(ruleList, Words.Magic | Words.Normal).Any());
            Assert.False(enumAnalyzer.Analyze(ruleList, Words.Shazam).Any());
            Assert.False(enumAnalyzer.Analyze(ruleList, Words.Normal).Any());

            Assert.True(enumAnalyzer.Analyze(ruleList, null, Words.Magic | Words.Normal).Any());
            Assert.False(enumAnalyzer.Analyze(ruleList, null, Words.Shazam).Any());
            Assert.False(enumAnalyzer.Analyze(ruleList, null, Words.Normal).Any());
        }

        [Fact]
        public void VerifyEndsWithOperator()
        {
            var trueEndsWithObject = "ThisStringEndsWithMagic";
            var falseEndsWithObject = "ThisStringHasMagicButDoesn't";

            var endsWithRule = new Rule("Ends With Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.EndsWith)
                    {
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    }
                }
            };

            var endsWithAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { endsWithRule };

            Assert.True(endsWithAnalyzer.Analyze(ruleList, trueEndsWithObject).Any());
            Assert.False(endsWithAnalyzer.Analyze(ruleList, falseEndsWithObject).Any());

            Assert.True(endsWithAnalyzer.Analyze(ruleList, null, trueEndsWithObject).Any());
            Assert.False(endsWithAnalyzer.Analyze(ruleList, null, falseEndsWithObject).Any());
        }

        [Fact]
        public void VerifyEqOperator()
        {
            var assertTrueObject = new TestObject()
            {
                StringField = "Magic",
                BoolField = true,
                IntField = 700
            };

            var assertFalseObject = new TestObject()
            {
                StringField = "NotMagic",
                BoolField = false,
                IntField = 701
            };

            var stringEquals = new Rule("String Equals Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    }
                }
            };

            var boolEquals = new Rule("Bool Equals Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "BoolField")
                    {
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };

            var intEquals = new Rule("Int Equals Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "IntField")
                    {
                        Data = new List<string>()
                        {
                            "700"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { boolEquals, intEquals, stringEquals };

            var trueObjectResults = analyzer.Analyze(ruleList, assertTrueObject);
            var falseObjectResults = analyzer.Analyze(ruleList, assertFalseObject);

            Assert.Contains(trueObjectResults, x => x.Name == "Bool Equals Rule");
            Assert.Contains(trueObjectResults, x => x.Name == "Int Equals Rule");
            Assert.Contains(trueObjectResults, x => x.Name == "String Equals Rule");

            Assert.DoesNotContain(falseObjectResults, x => x.Name == "Bool Equals Rule");
            Assert.DoesNotContain(falseObjectResults, x => x.Name == "Int Equals Rule");
            Assert.DoesNotContain(falseObjectResults, x => x.Name == "String Equals Rule");

            trueObjectResults = analyzer.Analyze(ruleList, null, assertTrueObject);
            falseObjectResults = analyzer.Analyze(ruleList, null, assertFalseObject);

            Assert.Contains(trueObjectResults, x => x.Name == "Bool Equals Rule");
            Assert.Contains(trueObjectResults, x => x.Name == "Int Equals Rule");
            Assert.Contains(trueObjectResults, x => x.Name == "String Equals Rule");

            Assert.DoesNotContain(falseObjectResults, x => x.Name == "Bool Equals Rule");
            Assert.DoesNotContain(falseObjectResults, x => x.Name == "Int Equals Rule");
            Assert.DoesNotContain(falseObjectResults, x => x.Name == "String Equals Rule");
        }

        [Fact]
        public void VerifyGtOperator()
        {
            var trueGtObject = new TestObject()
            {
                IntField = 9001
            };
            var falseGtObject = new TestObject()
            {
                IntField = 9000
            };

            var gtRule = new Rule("Gt Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.GreaterThan, "IntField")
                    {
                        Data = new List<string>()
                        {
                            "9000"
                        }
                    }
                }
            };

            var badGtRule = new Rule("Bad Gt Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.GreaterThan, "IntField")
                    {
                        Data = new List<string>()
                        {
                            "CONTOSO"
                        }
                    }
                }
            };

            var gtAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { gtRule };

            Assert.True(gtAnalyzer.Analyze(ruleList, trueGtObject).Any());
            Assert.False(gtAnalyzer.Analyze(ruleList, falseGtObject).Any());

            Assert.True(gtAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.False(gtAnalyzer.Analyze(ruleList, null, falseGtObject).Any());

            var badGtAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { badGtRule };

            Assert.False(badGtAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.False(badGtAnalyzer.Analyze(ruleList, null, falseGtObject).Any());
        }

        [Fact]
        public void VerifyIsAfterOperator()
        {
            var falseIsAfterObject = DateTime.MinValue;

            var trueIsAfterObject = DateTime.MaxValue;

            var isAfterRule = new Rule("Is After Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsAfter)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToString()
                        }
                    }
                }
            };

            var isAfterAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { isAfterRule };

            Assert.True(isAfterAnalyzer.Analyze(ruleList, trueIsAfterObject).Any());
            Assert.False(isAfterAnalyzer.Analyze(ruleList, falseIsAfterObject).Any());

            Assert.True(isAfterAnalyzer.Analyze(ruleList, null, trueIsAfterObject).Any());
            Assert.False(isAfterAnalyzer.Analyze(ruleList, null, falseIsAfterObject).Any());

            var isAfterRuleShortDate = new Rule("Is After Short Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsAfter)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToShortDateString()
                        }
                    }
                }
            };

            ruleList = new List<Rule>() { isAfterRuleShortDate };

            Assert.True(isAfterAnalyzer.Analyze(ruleList, trueIsAfterObject).Any());
            Assert.False(isAfterAnalyzer.Analyze(ruleList, falseIsAfterObject).Any());

            Assert.True(isAfterAnalyzer.Analyze(ruleList, null, trueIsAfterObject).Any());
            Assert.False(isAfterAnalyzer.Analyze(ruleList, null, falseIsAfterObject).Any());
        }

        [Fact]
        public void VerifyIsBeforeOperator()
        {
            var falseIsBeforeObject = DateTime.MaxValue;

            var falseIsAfterObject = DateTime.MinValue;

            var isBeforeRule = new Rule("Is Before Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsBefore)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToString()
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { isBeforeRule };

            Assert.True(analyzer.Analyze(ruleList, falseIsAfterObject).Any());
            Assert.False(analyzer.Analyze(ruleList, falseIsBeforeObject).Any());

            Assert.True(analyzer.Analyze(ruleList, null, falseIsAfterObject).Any());
            Assert.False(analyzer.Analyze(ruleList, null, falseIsBeforeObject).Any());

            var isBeforeShortRule = new Rule("Is Before Short Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsBefore)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToShortDateString()
                        }
                    }
                }
            };

            ruleList = new List<Rule>() { isBeforeShortRule };

            Assert.True(analyzer.Analyze(ruleList, falseIsAfterObject).Any());
            Assert.False(analyzer.Analyze(ruleList, falseIsBeforeObject).Any());

            Assert.True(analyzer.Analyze(ruleList, null, falseIsAfterObject).Any());
            Assert.False(analyzer.Analyze(ruleList, null, falseIsBeforeObject).Any());
        }

        [Fact]
        public void VerifyIsExpiredOperation()
        {
            var falseIsExpiredObject = DateTime.MaxValue;

            var trueIsExpiredObject = DateTime.MinValue;

            var isExpiredRule = new Rule("Is Expired Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsExpired)
                }
            };

            var isAfterAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { isExpiredRule };

            Assert.True(isAfterAnalyzer.Analyze(ruleList, trueIsExpiredObject).Any());
            Assert.False(isAfterAnalyzer.Analyze(ruleList, falseIsExpiredObject).Any());

            Assert.True(isAfterAnalyzer.Analyze(ruleList, null, trueIsExpiredObject).Any());
            Assert.False(isAfterAnalyzer.Analyze(ruleList, null, falseIsExpiredObject).Any());
        }

        [Fact]
        public void VerifyIsNullOperator()
        {
            var isNullRule = new Rule("Is Null Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull)
                }
            };

            var isNullAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { isNullRule };

            Assert.True(isNullAnalyzer.Analyze(ruleList, null).Any());
            Assert.False(isNullAnalyzer.Analyze(ruleList, "Not Null").Any());

            Assert.True(isNullAnalyzer.Analyze(ruleList, null, null).Any());
            Assert.False(isNullAnalyzer.Analyze(ruleList, null, "Not Null").Any());
        }

        [Fact]
        public void VerifyIsTrueOperator()
        {
            var isTrueRule = new Rule("Is True Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsTrue)
                }
            };

            var isTrueAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { isTrueRule };

            Assert.True(isTrueAnalyzer.Analyze(ruleList, true).Any());
            Assert.False(isTrueAnalyzer.Analyze(ruleList, false).Any());

            Assert.True(isTrueAnalyzer.Analyze(ruleList, null, true).Any());
            Assert.False(isTrueAnalyzer.Analyze(ruleList, null, false).Any());
        }

        [Fact]
        public void VerifyLtOperator()
        {
            var trueGtObject = new TestObject()
            {
                IntField = 8999
            };
            var falseGtObject = new TestObject()
            {
                IntField = 9000
            };

            var gtRule = new Rule("Lt Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.LessThan, "IntField")
                    {
                        Data = new List<string>()
                        {
                            "9000"
                        }
                    }
                }
            };

            var badGtRule = new Rule("Bad Lt Rule")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.LessThan, "IntField")
                    {
                        Data = new List<string>()
                        {
                            "CONTOSO"
                        }
                    }
                }
            };

            var ltAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { gtRule };

            Assert.True(ltAnalyzer.Analyze(ruleList, trueGtObject).Any());
            Assert.False(ltAnalyzer.Analyze(ruleList, falseGtObject).Any());

            Assert.True(ltAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.False(ltAnalyzer.Analyze(ruleList, null, falseGtObject).Any());

            ruleList = new List<Rule>() { badGtRule };

            Assert.False(ltAnalyzer.Analyze(ruleList, trueGtObject).Any());
            Assert.False(ltAnalyzer.Analyze(ruleList, falseGtObject).Any());

            Assert.False(ltAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.False(ltAnalyzer.Analyze(ruleList, null, falseGtObject).Any());
        }

        [Fact]
        public void VerifyRegexOperator()
        {
            var falseRegexObject = "TestPathHere";
            var trueRegexObject = "Directory/File";

            var regexRule = new Rule("Regex Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Regex)
                    {
                        Data = new List<string>()
                        {
                            ".+\\/.+"
                        }
                    }
                }
            };

            var regexAnalyzer = new Analyzer();
            var ruleList = new List<Rule>() { regexRule };

            Assert.True(regexAnalyzer.Analyze(ruleList, trueRegexObject).Any());
            Assert.False(regexAnalyzer.Analyze(ruleList, falseRegexObject).Any());

            Assert.True(regexAnalyzer.Analyze(ruleList, null, trueRegexObject).Any());
            Assert.False(regexAnalyzer.Analyze(ruleList, null, falseRegexObject).Any());
        }

        [Fact]
        public void VerifyStartsWithOperator()
        {
            var trueEndsWithObject = "MagicStartsThisStringOff";
            var falseEndsWithObject = "ThisStringHasMagicButLater";

            var startsWithRule = new Rule("Starts With Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.StartsWith)
                    {
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { startsWithRule };

            Assert.True(analyzer.Analyze(ruleList, trueEndsWithObject).Any());
            Assert.False(analyzer.Analyze(ruleList, falseEndsWithObject).Any());

            Assert.True(analyzer.Analyze(ruleList, null, trueEndsWithObject).Any());
            Assert.False(analyzer.Analyze(ruleList, null, falseEndsWithObject).Any());
        }

        [Fact]
        public void VerifyWasModifiedOperator()
        {
            var firstObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>() { { "Magic Word", "Please" }, { "Another Key", "Another Value" } }
            };

            var secondObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>() { { "Magic Word", "Abra Kadabra" }, { "Another Key", "A Different Value" } }
            };

            var wasModifiedRule = new Rule("Was Modified Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.WasModified)
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { wasModifiedRule };

            Assert.True(analyzer.Analyze(ruleList, true, false).Any());
            Assert.True(analyzer.Analyze(ruleList, "A String", "Another string").Any());
            Assert.True(analyzer.Analyze(ruleList, 3, 4).Any());
            Assert.True(analyzer.Analyze(ruleList, new List<string>() { "One", "Two" }, new List<string>() { "Three", "Four" }).Any());
            Assert.True(analyzer.Analyze(ruleList, firstObject, secondObject).Any());

            Assert.False(analyzer.Analyze(ruleList, true, true).Any());
            Assert.False(analyzer.Analyze(ruleList, "A String", "A String").Any());
            Assert.False(analyzer.Analyze(ruleList, 3, 3).Any());
            Assert.False(analyzer.Analyze(ruleList, new List<string>() { "One", "Two" }, new List<string>() { "One", "Two" }).Any());
            Assert.False(analyzer.Analyze(ruleList, firstObject, firstObject).Any());
        }

        [Flags]
        private enum Words
        {
            Normal = 1,
            Magic = 2,
            Shazam = 4
        }
    }
}