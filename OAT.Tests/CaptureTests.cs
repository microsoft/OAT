using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        }
    }
}
