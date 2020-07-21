using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class CaptureTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        private TestObject testObjectTrueFalse = new TestObject()
        {
            StringField = "MagicWord",
            BoolField = false
        };

        [TestMethod]
        public void TestEQCapture()
        {
            var RuleName = "Equals Capture";
            var eqCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.EQ, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { eqCaptureRule };

            Assert.IsTrue(analyzer.GetCapture(eqCaptureRule, testObjectTrueFalse, null).Result?.Captures.Any(x => x is StringCapture y && y.Result == "MagicWord") is true);
        }

        [TestMethod]
        public void TestNEQCapture()
        {
            var RuleName = "Equals Capture";
            var neqCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.NEQ, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "Contoso"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { neqCaptureRule };

            Assert.IsTrue(analyzer.GetCapture(neqCaptureRule, testObjectTrueFalse, null).Result?.Captures.Any(x => x is StringCapture y && y.Result == "MagicWord") is true);
        }

        [TestMethod]
        public void TestLTCapture()
        {
            var RuleName = "LT Capture";
            var ltCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.LT)
                    {
                        Data = new List<string>()
                        {
                            "5"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { ltCaptureRule };

            Assert.IsTrue(analyzer.GetCapture(ltCaptureRule, 3, null).Result?.Captures.Any(x => x is IntCapture y && y.Result == 3) is true);
        }

        [TestMethod]
        public void TestGTCapture()
        {
            var RuleName = "GT Capture";
            var gtCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.GT)
                    {
                        Data = new List<string>()
                        {
                            "5"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { gtCaptureRule };

            Assert.IsTrue(analyzer.GetCapture(gtCaptureRule, 7, null).Result?.Captures.Any(x => x is IntCapture y && y.Result == 7) is true);
        }

        [Flags]
        enum Words
        {
            None = 0,
            Magic = 1,
            Words = 2
        }

        [TestMethod]
        public void TestContainsCapture()
        {
            var RuleName = "Contains Capture";
            var containsCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS)
                    {
                        Data = new List<string>()
                        {
                            "Magic",
                            "Words"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { containsCaptureRule };

            var res = analyzer.GetCapture(containsCaptureRule, "ThisStringContainsSomeMagicWords", null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is StringCapture y && y.Result == "ThisStringContainsSomeMagicWords") is true);

            res = analyzer.GetCapture(containsCaptureRule, new List<string>() { "Magic", "Words" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Contains("Magic")) is true);

            res = analyzer.GetCapture(containsCaptureRule, Words.Magic | Words.Words, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is EnumCapture y && y.Result.HasFlag(Words.Magic)) is true);

            var testdata = new Dictionary<string, string>() { { "Version", "1.0" }, { "State", "Beta" } };

            containsCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS)
                    {
                        DictData = testdata.ToList(),
                        Capture = true
                    }
                }
            };

            res = analyzer.GetCapture(containsCaptureRule, testdata, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string,string> y && y.Result.Any(x => x.Key == "Version")) is true);

            var testlist = new Dictionary<string, string>() { { "Version", "1.0" }, { "State", "Beta" } }.ToList();

            res = analyzer.GetCapture(containsCaptureRule, testlist, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Version")) is true);
        }

        [TestMethod]
        public void TestContainsAnyCapture()
        {
            var RuleName = "Contains Any Capture";
            var containsAnyCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS_ANY)
                    {
                        Data = new List<string>()
                        {
                            "Magic",
                            "Words",
                            "Kaboom"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { containsAnyCaptureRule };

            var res = analyzer.GetCapture(containsAnyCaptureRule, "ThisStringContainsSomeMagic", null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is StringCapture y && y.Result == "ThisStringContainsSomeMagic") is true);

            res = analyzer.GetCapture(containsAnyCaptureRule, new List<string>() { "Magic", "Words" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Contains("Magic")) is true);

            res = analyzer.GetCapture(containsAnyCaptureRule, Words.Magic | Words.Words, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is EnumCapture y && y.Result.HasFlag(Words.Magic)) is true);

            var testdata = new Dictionary<string, string>() { { "Version", "1.0" }, { "State", "Beta" } };

            containsAnyCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS_ANY)
                    {
                        DictData = testdata.ToList(),
                        Capture = true
                    }
                }
            };

            testdata.Remove("State");

            res = analyzer.GetCapture(containsAnyCaptureRule, testdata, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Version")) is true);

            var testlist = testdata.ToList();

            res = analyzer.GetCapture(containsAnyCaptureRule, testlist, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Version")) is true);
        }
    }
}
