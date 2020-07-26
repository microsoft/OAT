// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class OperationsTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        [TestMethod]
        public void VerifyContainsAnyOperator()
        {
            TestObject? trueStringObject = new TestObject()
            {
                StringField = "ThisStringContainsMagic"
            };

            TestObject? falseStringObject = new TestObject()
            {
                StringField = "ThisStringDoesNot"
            };

            Rule? stringContains = new Rule("String Contains Any Rule")
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

            Analyzer? stringAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { stringContains };

            Assert.IsTrue(stringAnalyzer.Analyze(ruleList, trueStringObject).Any());
            Assert.IsFalse(stringAnalyzer.Analyze(ruleList, falseStringObject).Any());

            Assert.IsTrue(stringAnalyzer.Analyze(ruleList, null, trueStringObject).Any());
            Assert.IsFalse(stringAnalyzer.Analyze(ruleList, null, falseStringObject).Any());

            TestObject? trueListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Magic",
                    "Three"
                }
            };

            TestObject? falseListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Two",
                    "Three"
                }
            };

            Rule? listContains = new Rule("List Contains Any Rule")
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

            Analyzer? listAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listContains };

            Assert.IsTrue(listAnalyzer.Analyze(ruleList, trueListObject).Any());
            Assert.IsFalse(listAnalyzer.Analyze(ruleList, falseListObject).Any());

            Assert.IsTrue(listAnalyzer.Analyze(ruleList, null, trueListObject).Any());
            Assert.IsFalse(listAnalyzer.Analyze(ruleList, null, falseListObject).Any());

            TestObject? trueStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Hocus Pocus" },
                    { "Two", "Abra Kadabra" },
                    { "Magic Word", "Please" }
                }
            };

            TestObject? falseStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Magic" },
                    { "Two", "Show" },
                    { "Three", "Please" }
                }
            };

            Rule? stringDictContains = new Rule("String Dict Contains Any Rule")
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

            Analyzer? stringDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { stringDictContains };

            Assert.IsTrue(stringDictAnalyzer.Analyze(ruleList, trueStringDictObject).Any());
            Assert.IsFalse(stringDictAnalyzer.Analyze(ruleList, falseStringDictObject).Any());

            Assert.IsTrue(stringDictAnalyzer.Analyze(ruleList, null, trueStringDictObject).Any());
            Assert.IsFalse(stringDictAnalyzer.Analyze(ruleList, null, falseStringDictObject).Any());

            TestObject? trueListDictObject = new TestObject()
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

            TestObject? falseListDictObject = new TestObject()
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

            Rule? listDictContains = new Rule("List Dict Contains Any Rule")
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

            Analyzer? listDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listDictContains };

            Assert.IsTrue(listDictAnalyzer.Analyze(ruleList, trueListDictObject).Any());
            Assert.IsFalse(listDictAnalyzer.Analyze(ruleList, falseListDictObject).Any());

            Assert.IsTrue(listDictAnalyzer.Analyze(ruleList, null, trueListDictObject).Any());
            Assert.IsFalse(listDictAnalyzer.Analyze(ruleList, null, falseListDictObject).Any());

            Rule? enumFlagsContains = new Rule("Enum Flags Contains Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.ContainsAny)
                    {
                        Data = new List<string>(){"Magic", "Normal"}
                    }
                }
            };

            Analyzer? enumAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { enumFlagsContains };

            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, Words.Magic).Any());
            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, Words.Normal).Any());
            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, Words.Magic | Words.Normal).Any());
            Assert.IsFalse(enumAnalyzer.Analyze(ruleList, Words.Shazam).Any());

            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, null, Words.Magic).Any());
            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, null, Words.Normal).Any());
            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, null, Words.Magic | Words.Normal).Any());
            Assert.IsFalse(enumAnalyzer.Analyze(ruleList, null, Words.Shazam).Any());
        }

        [TestMethod]
        public void VerifyContainsOperator()
        {
            TestObject? trueStringObject = new TestObject()
            {
                StringField = "ThisStringContainsMagic"
            };

            TestObject? falseStringObject = new TestObject()
            {
                StringField = "ThisOneDoesNot"
            };

            Rule? stringContains = new Rule("String Contains Rule")
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

            Analyzer? stringAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { stringContains };

            Assert.IsTrue(stringAnalyzer.Analyze(ruleList, trueStringObject).Any());
            Assert.IsFalse(stringAnalyzer.Analyze(ruleList, falseStringObject).Any());

            Assert.IsTrue(stringAnalyzer.Analyze(ruleList, null, trueStringObject).Any());
            Assert.IsFalse(stringAnalyzer.Analyze(ruleList, null, falseStringObject).Any());

            TestObject? trueListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Magic",
                    "Abra Kadabra"
                }
            };

            TestObject? falseListObject = new TestObject()
            {
                StringListField = new List<string>()
                {
                    "One",
                    "Two",
                    "Three"
                }
            };

            Rule? listContains = new Rule("List Contains Rule")
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

            Analyzer? listAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listContains };

            Assert.IsTrue(listAnalyzer.Analyze(ruleList, trueListObject).Any());
            Assert.IsFalse(listAnalyzer.Analyze(ruleList, falseListObject).Any());

            Assert.IsTrue(listAnalyzer.Analyze(ruleList, null, trueListObject).Any());
            Assert.IsFalse(listAnalyzer.Analyze(ruleList, null, falseListObject).Any());

            TestObject? trueStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Money" },
                    { "Two", "Show" },
                    { "Magic Word", "Please" }
                }
            };

            TestObject? falseStringDictObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Magic" },
                    { "Two", "Something" },
                    { "Three", "Please" }
                }
            };

            Rule? stringDictContains = new Rule("String Dict Contains Rule")
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

            Analyzer? stringDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { stringDictContains };

            Assert.IsTrue(stringDictAnalyzer.Analyze(ruleList, trueStringDictObject).Any());
            Assert.IsFalse(stringDictAnalyzer.Analyze(ruleList, falseStringDictObject).Any());

            Assert.IsTrue(stringDictAnalyzer.Analyze(ruleList, null, trueStringDictObject).Any());
            Assert.IsFalse(stringDictAnalyzer.Analyze(ruleList, null, falseStringDictObject).Any());

            TestObject? trueListDictObject = new TestObject()
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

            TestObject? falseListDictObject = new TestObject()
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

            Rule? listDictContains = new Rule("List Dict Contains Rule")
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

            Analyzer? listDictAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { listDictContains };

            Assert.IsTrue(listDictAnalyzer.Analyze(ruleList, trueListDictObject).Any());
            Assert.IsFalse(listDictAnalyzer.Analyze(ruleList, falseListDictObject).Any());

            Assert.IsTrue(listDictAnalyzer.Analyze(ruleList, null, trueListDictObject).Any());
            Assert.IsFalse(listDictAnalyzer.Analyze(ruleList, null, falseListDictObject).Any());

            Rule? enumFlagsContains = new Rule("Enum Flags Contains Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Contains)
                    {
                        Data = new List<string>(){"Magic", "Normal"}
                    }
                }
            };

            Analyzer? enumAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { enumFlagsContains };

            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, Words.Magic | Words.Normal).Any());
            Assert.IsFalse(enumAnalyzer.Analyze(ruleList, Words.Shazam).Any());
            Assert.IsFalse(enumAnalyzer.Analyze(ruleList, Words.Normal).Any());

            Assert.IsTrue(enumAnalyzer.Analyze(ruleList, null, Words.Magic | Words.Normal).Any());
            Assert.IsFalse(enumAnalyzer.Analyze(ruleList, null, Words.Shazam).Any());
            Assert.IsFalse(enumAnalyzer.Analyze(ruleList, null, Words.Normal).Any());
        }

        [Flags]
        enum Words
        {
            Normal = 1,
            Magic = 2,
            Shazam = 4
        }

        [TestMethod]
        public void VerifyContainsKeyOperator()
        {
            TestObject? trueAlgDict = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "Magic", "Anything" }
                }
            };

            TestObject? falseAlgDict = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "No Magic", "Anything" }
                }
            };

            Rule? dictContainsKey = new Rule("DictContainsKey")
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

            Analyzer? algDictAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { dictContainsKey };

            Assert.IsTrue(algDictAnalyzer.Analyze(ruleList, trueAlgDict).Any());
            Assert.IsFalse(algDictAnalyzer.Analyze(ruleList, falseAlgDict).Any());

            Assert.IsTrue(algDictAnalyzer.Analyze(ruleList, null, trueAlgDict).Any());
            Assert.IsFalse(algDictAnalyzer.Analyze(ruleList, null, falseAlgDict).Any());
        }

        [TestMethod]
        public void VerifyEndsWithOperator()
        {
            string? trueEndsWithObject = "ThisStringEndsWithMagic";
            string? falseEndsWithObject = "ThisStringHasMagicButDoesn't";

            Rule? endsWithRule = new Rule("Ends With Rule")
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

            Analyzer? endsWithAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { endsWithRule };

            Assert.IsTrue(endsWithAnalyzer.Analyze(ruleList, trueEndsWithObject).Any());
            Assert.IsFalse(endsWithAnalyzer.Analyze(ruleList, falseEndsWithObject).Any());

            Assert.IsTrue(endsWithAnalyzer.Analyze(ruleList, null, trueEndsWithObject).Any());
            Assert.IsFalse(endsWithAnalyzer.Analyze(ruleList, null, falseEndsWithObject).Any());
        }

        [TestMethod]
        public void VerifyStartsWithOperator()
        {
            string? trueEndsWithObject = "MagicStartsThisStringOff";
            string? falseEndsWithObject = "ThisStringHasMagicButLater";

            Rule? startsWithRule = new Rule("Starts With Rule")
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

            Analyzer? analyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { startsWithRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, trueEndsWithObject).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, falseEndsWithObject).Any());

            Assert.IsTrue(analyzer.Analyze(ruleList, null, trueEndsWithObject).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, null, falseEndsWithObject).Any());
        }

        [TestMethod]
        public void VerifyEqOperator()
        {
            TestObject? assertTrueObject = new TestObject()
            {
                StringField = "Magic",
                BoolField = true,
                IntField = 700
            };

            TestObject? assertFalseObject = new TestObject()
            {
                StringField = "NotMagic",
                BoolField = false,
                IntField = 701
            };

            Rule? stringEquals = new Rule("String Equals Rule")
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

            Rule? boolEquals = new Rule("Bool Equals Rule")
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

            Rule? intEquals = new Rule("Int Equals Rule")
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

            Analyzer? analyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { boolEquals, intEquals, stringEquals };

            IEnumerable<Rule>? trueObjectResults = analyzer.Analyze(ruleList, assertTrueObject);
            IEnumerable<Rule>? falseObjectResults = analyzer.Analyze(ruleList, assertFalseObject);

            Assert.IsTrue(trueObjectResults.Any(x => x.Name == "Bool Equals Rule"));
            Assert.IsTrue(trueObjectResults.Any(x => x.Name == "Int Equals Rule"));
            Assert.IsTrue(trueObjectResults.Any(x => x.Name == "String Equals Rule"));

            Assert.IsFalse(falseObjectResults.Any(x => x.Name == "Bool Equals Rule"));
            Assert.IsFalse(falseObjectResults.Any(x => x.Name == "Int Equals Rule"));
            Assert.IsFalse(falseObjectResults.Any(x => x.Name == "String Equals Rule"));

            trueObjectResults = analyzer.Analyze(ruleList, null, assertTrueObject);
            falseObjectResults = analyzer.Analyze(ruleList, null, assertFalseObject);

            Assert.IsTrue(trueObjectResults.Any(x => x.Name == "Bool Equals Rule"));
            Assert.IsTrue(trueObjectResults.Any(x => x.Name == "Int Equals Rule"));
            Assert.IsTrue(trueObjectResults.Any(x => x.Name == "String Equals Rule"));

            Assert.IsFalse(falseObjectResults.Any(x => x.Name == "Bool Equals Rule"));
            Assert.IsFalse(falseObjectResults.Any(x => x.Name == "Int Equals Rule"));
            Assert.IsFalse(falseObjectResults.Any(x => x.Name == "String Equals Rule"));
        }

        [TestMethod]
        public void VerifyGtOperator()
        {
            TestObject? trueGtObject = new TestObject()
            {
                IntField = 9001
            };
            TestObject? falseGtObject = new TestObject()
            {
                IntField = 9000
            };

            Rule? gtRule = new Rule("Gt Rule")
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

            Rule? badGtRule = new Rule("Bad Gt Rule")
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

            Analyzer? gtAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { gtRule };

            Assert.IsTrue(gtAnalyzer.Analyze(ruleList, trueGtObject).Any());
            Assert.IsFalse(gtAnalyzer.Analyze(ruleList, falseGtObject).Any());

            Assert.IsTrue(gtAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.IsFalse(gtAnalyzer.Analyze(ruleList, null, falseGtObject).Any());

            Analyzer? badGtAnalyzer = new Analyzer();
            ruleList = new List<Rule>() { badGtRule };

            Assert.IsFalse(badGtAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.IsFalse(badGtAnalyzer.Analyze(ruleList, null, falseGtObject).Any());
        }

        [TestMethod]
        public void VerifyLtOperator()
        {
            TestObject? trueGtObject = new TestObject()
            {
                IntField = 8999
            };
            TestObject? falseGtObject = new TestObject()
            {
                IntField = 9000
            };

            Rule? gtRule = new Rule("Lt Rule")
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

            Rule? badGtRule = new Rule("Bad Lt Rule")
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

            Analyzer? ltAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { gtRule };

            Assert.IsTrue(ltAnalyzer.Analyze(ruleList, trueGtObject).Any());
            Assert.IsFalse(ltAnalyzer.Analyze(ruleList, falseGtObject).Any());

            Assert.IsTrue(ltAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.IsFalse(ltAnalyzer.Analyze(ruleList, null, falseGtObject).Any());

            ruleList = new List<Rule>() { badGtRule };

            Assert.IsFalse(ltAnalyzer.Analyze(ruleList, trueGtObject).Any());
            Assert.IsFalse(ltAnalyzer.Analyze(ruleList, falseGtObject).Any());

            Assert.IsFalse(ltAnalyzer.Analyze(ruleList, null, trueGtObject).Any());
            Assert.IsFalse(ltAnalyzer.Analyze(ruleList, null, falseGtObject).Any());
        }

        [TestMethod]
        public void VerifyIsAfterOperator()
        {
            DateTime falseIsAfterObject = DateTime.MinValue;

            DateTime trueIsAfterObject = DateTime.MaxValue;

            Rule? isAfterRule = new Rule("Is After Rule")
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

            Analyzer? isAfterAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { isAfterRule };

            Assert.IsTrue(isAfterAnalyzer.Analyze(ruleList, trueIsAfterObject).Any());
            Assert.IsFalse(isAfterAnalyzer.Analyze(ruleList, falseIsAfterObject).Any());

            Assert.IsTrue(isAfterAnalyzer.Analyze(ruleList, null, trueIsAfterObject).Any());
            Assert.IsFalse(isAfterAnalyzer.Analyze(ruleList, null, falseIsAfterObject).Any());

            Rule? isAfterRuleShortDate = new Rule("Is After Short Rule")
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

            Assert.IsTrue(isAfterAnalyzer.Analyze(ruleList, trueIsAfterObject).Any());
            Assert.IsFalse(isAfterAnalyzer.Analyze(ruleList, falseIsAfterObject).Any());

            Assert.IsTrue(isAfterAnalyzer.Analyze(ruleList, null, trueIsAfterObject).Any());
            Assert.IsFalse(isAfterAnalyzer.Analyze(ruleList, null, falseIsAfterObject).Any());

        }

        [TestMethod]
        public void VerifyIsBeforeOperator()
        {
            DateTime falseIsBeforeObject = DateTime.MaxValue;

            DateTime falseIsAfterObject = DateTime.MinValue;

            Rule? isBeforeRule = new Rule("Is Before Rule")
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

            Analyzer? analyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { isBeforeRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, falseIsAfterObject).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, falseIsBeforeObject).Any());

            Assert.IsTrue(analyzer.Analyze(ruleList, null, falseIsAfterObject).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, null, falseIsBeforeObject).Any());

            Rule? isBeforeShortRule = new Rule("Is Before Short Rule")
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

            Assert.IsTrue(analyzer.Analyze(ruleList, falseIsAfterObject).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, falseIsBeforeObject).Any());

            Assert.IsTrue(analyzer.Analyze(ruleList, null, falseIsAfterObject).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, null, falseIsBeforeObject).Any());
        }

        [TestMethod]
        public void VerifyIsExpiredOperation()
        {
            DateTime falseIsExpiredObject = DateTime.MaxValue;

            DateTime trueIsExpiredObject = DateTime.MinValue;

            Rule? isExpiredRule = new Rule("Is Expired Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsExpired)
                }
            };

            Analyzer? isAfterAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { isExpiredRule };

            Assert.IsTrue(isAfterAnalyzer.Analyze(ruleList, trueIsExpiredObject).Any());
            Assert.IsFalse(isAfterAnalyzer.Analyze(ruleList, falseIsExpiredObject).Any());

            Assert.IsTrue(isAfterAnalyzer.Analyze(ruleList, null, trueIsExpiredObject).Any());
            Assert.IsFalse(isAfterAnalyzer.Analyze(ruleList, null, falseIsExpiredObject).Any());
        }

        [TestMethod]
        public void VerifyIsNullOperator()
        {

            Rule? isNullRule = new Rule("Is Null Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull)
                }
            };

            Analyzer? isNullAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { isNullRule };

            Assert.IsTrue(isNullAnalyzer.Analyze(ruleList, null).Any());
            Assert.IsFalse(isNullAnalyzer.Analyze(ruleList, "Not Null").Any());

            Assert.IsTrue(isNullAnalyzer.Analyze(ruleList, null, null).Any());
            Assert.IsFalse(isNullAnalyzer.Analyze(ruleList, null, "Not Null").Any());
        }

        [TestMethod]
        public void VerifyIsTrueOperator()
        {
            Rule? isTrueRule = new Rule("Is True Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsTrue)
                }
            };

            Analyzer? isTrueAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { isTrueRule };

            Assert.IsTrue(isTrueAnalyzer.Analyze(ruleList, true).Any());
            Assert.IsFalse(isTrueAnalyzer.Analyze(ruleList, false).Any());

            Assert.IsTrue(isTrueAnalyzer.Analyze(ruleList, null, true).Any());
            Assert.IsFalse(isTrueAnalyzer.Analyze(ruleList, null, false).Any());
        }

        [TestMethod]
        public void VerifyRegexOperator()
        {
            string? falseRegexObject = "TestPathHere";
            string? trueRegexObject = "Directory/File";

            Rule? regexRule = new Rule("Regex Rule")
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

            Analyzer? regexAnalyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { regexRule };

            Assert.IsTrue(regexAnalyzer.Analyze(ruleList, trueRegexObject).Any());
            Assert.IsFalse(regexAnalyzer.Analyze(ruleList, falseRegexObject).Any());

            Assert.IsTrue(regexAnalyzer.Analyze(ruleList, null, trueRegexObject).Any());
            Assert.IsFalse(regexAnalyzer.Analyze(ruleList, null, falseRegexObject).Any());
        }

        [TestMethod]
        public void VerifyWasModifiedOperator()
        {
            TestObject? firstObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>() { { "Magic Word", "Please" }, { "Another Key", "Another Value" } }
            };

            TestObject? secondObject = new TestObject()
            {
                StringDictField = new Dictionary<string, string>() { { "Magic Word", "Abra Kadabra" }, { "Another Key", "A Different Value" } }
            };

            Rule? wasModifiedRule = new Rule("Was Modified Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.WasModified)
                }
            };

            Analyzer? analyzer = new Analyzer();
            List<Rule>? ruleList = new List<Rule>() { wasModifiedRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, true, false).Any());
            Assert.IsTrue(analyzer.Analyze(ruleList, "A String", "Another string").Any());
            Assert.IsTrue(analyzer.Analyze(ruleList, 3, 4).Any());
            Assert.IsTrue(analyzer.Analyze(ruleList, new List<string>() { "One", "Two" }, new List<string>() { "Three", "Four" }).Any());
            Assert.IsTrue(analyzer.Analyze(ruleList, firstObject, secondObject).Any());


            Assert.IsFalse(analyzer.Analyze(ruleList, true, true).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, "A String", "A String").Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, 3, 3).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, new List<string>() { "One", "Two" }, new List<string>() { "One", "Two" }).Any());
            Assert.IsFalse(analyzer.Analyze(ruleList, firstObject, firstObject).Any());
        }
    }
}