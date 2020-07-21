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
        public void TestEndsWithCapture()
        {
            var RuleName = "Ends With Capture";
            var endsWithCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.ENDS_WITH)
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
            var ruleList = new List<Rule>() { endsWithCapture };
            var res = analyzer.GetCapture(endsWithCapture, new List<string>() { "35", "47", "65" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Count == 2) is true);
        }

        [TestMethod]
        public void TestStartsWithCapture()
        {
            var RuleName = "Starts With Capture";
            var startsWithCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.STARTS_WITH)
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
            var ruleList = new List<Rule>() { startsWithCapture };
            var res = analyzer.GetCapture(startsWithCapture, new List<string>() { "53", "47", "56" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Count == 2) is true);
        }

        [TestMethod]
        public void TestNullCapture()
        {
            var RuleName = "Null Capture";
            var nullCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_NULL)
                    {
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { nullCapture };
            var res = analyzer.GetCapture(nullCapture, null, null);
            Assert.IsTrue(res.Result?.Captures is null);
        }

        [TestMethod]
        public void TestBoolCapture()
        {
            var RuleName = "Bool Capture";
            var boolCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_TRUE)
                    {
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { boolCapture };
            var res = analyzer.GetCapture(boolCapture, true, null);
            Assert.IsTrue(res.Result?.Captures.First() is BoolCapture x && x.Result);
        }

        [TestMethod]
        public void TestContainsKeyCapture()
        {
            var RuleName = "Contains Key Capture";
            var containsKeyRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS_KEY)
                    {
                        Data = new List<string>()
                        {
                            "Version",
                            "State"
                        },
                        Capture = true
                    }
                }
            };

            var testdata = new Dictionary<string, string>() { { "Version", "1.0" }, { "State", "Beta" } };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { containsKeyRule };
            var res = analyzer.GetCapture(containsKeyRule, testdata, null);
            Assert.IsTrue(res.Result?.Captures.First() is ListCapture<string> x && x.Result.Count == 2);
        }

        [TestMethod]
        public void TestIsExpiredCapture()
        {
            var RuleName = "Is Expired Capture";
            var isExpiredRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_EXPIRED)
                    {
                        Capture = true
                    }
                }
            };

            var timestamp = DateTime.Now.AddDays(-11);
            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { isExpiredRule };
            var res = analyzer.GetCapture(isExpiredRule, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture x && x.Result == timestamp);
        }

        [TestMethod]
        public void TestIsAfterCapture()
        {
            var RuleName = "IsAfter Capture";
            var isBeforeCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_AFTER)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToString()
                        },
                        Capture = true
                    }
                }
            };

            var timestamp = DateTime.Now.AddDays(1);
            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { isBeforeCapture };
            var res = analyzer.GetCapture(isBeforeCapture, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture x && x.Result == timestamp);
        }

        [TestMethod]
        public void TestIsBeforeCapture()
        {
            var RuleName = "IsBefore Capture";
            var isBeforeCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_BEFORE)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToString()
                        },
                        Capture = true
                    }
                }
            };

            var timestamp = DateTime.Now.AddDays(-1);
            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { isBeforeCapture };
            var res = analyzer.GetCapture(isBeforeCapture, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture x && x.Result == timestamp);
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
        public void TestWasModifiedCapture()
        {
            var RuleName = "WasModified Capture";
            var wasModifiedRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.WAS_MODIFIED)
                    {
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { wasModifiedRule };
            var testString = "The Secret is Magic";
            var testString2 = "The Secret is Science";
            var res = analyzer.GetCapture(wasModifiedRule, testString, testString2);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ComparisonResultCapture y && !y.Result.AreEqual) is true);
        }

        [TestMethod]
        public void TestRegexCapture()
        {
            var RuleName = "Regex Capture";
            var regexCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.REGEX)
                    {
                        Data = new List<string>()
                        {
                            "Secret.*([Mm]agic).*"
                        },
                        Capture = true
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { regexCaptureRule };
            var testString = "The Secret is Magic";
            var res = analyzer.GetCapture(regexCaptureRule, testString, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is RegexCapture y && y.Result.Groups[1].Value == "Magic") is true);
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
