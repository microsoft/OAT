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

        private const string CorrectString = "MagicWord";

        private TestObject testObjectTrueFalse = new TestObject()
        {
            StringField = CorrectString,
            BoolField = false
        };

        [TestMethod]
        public void TestEQCapture()
        {
            var RuleName = "Equals Capture";
            var analyzer = new Analyzer();

            var eqCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.EQ, "StringField")
                    {
                        Data = new List<string>()
                        {
                            CorrectString
                        },
                        Capture = true
                    }
                }
            };

            Assert.IsTrue(analyzer.GetCapture(eqCaptureRule, testObjectTrueFalse, null).Result?.Captures.Any(x => x is StringCapture y && y.Result == CorrectString) is true);

            eqCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.EQ, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "SomethingElse"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            Assert.IsTrue(analyzer.GetCapture(eqCaptureRule, testObjectTrueFalse, null).Result?.Captures.Any(x => x is StringCapture y && y.Result == CorrectString) is true);
        }

        [TestMethod]
        public void TestNEQCapture()
        {
            var RuleName = "Equals Capture";
            var analyzer = new Analyzer();

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

            Assert.IsTrue(analyzer.GetCapture(neqCaptureRule, testObjectTrueFalse, null).Result?.Captures.Any(x => x is StringCapture y && y.Result == CorrectString) is true);
            
            neqCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.NEQ, "StringField")
                    {
                        Data = new List<string>()
                        {
                            CorrectString
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            Assert.IsTrue(analyzer.GetCapture(neqCaptureRule, testObjectTrueFalse, null).Result?.Captures.Any(x => x is StringCapture y && y.Result == CorrectString) is true);
        }

        [TestMethod]
        public void TestEndsWithCapture()
        {
            var RuleName = "Ends With Capture";
            var analyzer = new Analyzer();

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

            var res = analyzer.GetCapture(endsWithCapture, new List<string>() { "35", "47", "65" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Count == 2) is true);

            endsWithCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.ENDS_WITH)
                    {
                        Data = new List<string>()
                        {
                            "5"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            res = analyzer.GetCapture(endsWithCapture, new List<string>() { "35", "47", "65" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is StringCapture y && y.Result == "47") is true);
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

            startsWithCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.STARTS_WITH)
                    {
                        Data = new List<string>()
                        {
                            "5"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            res = analyzer.GetCapture(startsWithCapture, new List<string>() { "53", "47", "56" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is StringCapture y && y.Result == "47") is true);
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
            var res = analyzer.GetCapture(nullCapture, null, null);
            Assert.IsTrue(res.Result?.Captures.Any() is true && res.RuleMatches);

            nullCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_NULL)
                    {
                        Capture = true,
                        Invert = true
                    }
                }
            };
            res = analyzer.GetCapture(nullCapture, null, null);
            Assert.IsTrue(res.Result?.Captures is null && !res.RuleMatches);
        }

        [TestMethod]
        public void TestIsTrueCapture()
        {
            var RuleName = "Bool Capture";
            var analyzer = new Analyzer();
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

            var res = analyzer.GetCapture(boolCapture, true, null);
            Assert.IsTrue(res.Result?.Captures.First() is BoolCapture x && x.Result);

            boolCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_TRUE)
                    {
                        Capture = true,
                        Invert = true
                    }
                }
            };
            res = analyzer.GetCapture(boolCapture, false, null);
            Assert.IsTrue(res.Result?.Captures.First() is BoolCapture y && !y.Result);
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
            
            var res = analyzer.GetCapture(containsKeyRule, testdata, null);

            Assert.IsTrue(res.Result?.Captures.First() is ListCapture<string> x && x.Result.Count == 2);
            containsKeyRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS_KEY)
                    {
                        Data = new List<string>()
                        {
                            "Taco",
                            "Hats"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };
            res = analyzer.GetCapture(containsKeyRule, testdata, null);

            Assert.IsTrue(res.Result?.Captures.First() is ListCapture<string> y && y.Result.Count == 2 && y.Result.Contains("Version"));
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

            var res = analyzer.GetCapture(isExpiredRule, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture x && x.Result == timestamp);

            isExpiredRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_EXPIRED)
                    {
                        Capture = true,
                        Invert = true
                    }
                }
            };

            timestamp = DateTime.Now.AddDays(1);

            res = analyzer.GetCapture(isExpiredRule, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture y && y.Result == timestamp);
        }

        [TestMethod]
        public void TestIsAfterCapture()
        {
            var RuleName = "IsAfter Capture";

            var analyzer = new Analyzer();

            var isAfterCapture = new Rule(RuleName)
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
            var res = analyzer.GetCapture(isAfterCapture, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture x && x.Result == timestamp);

            isAfterCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_AFTER)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToString()
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };


            timestamp = DateTime.Now.AddDays(-1);
            res = analyzer.GetCapture(isAfterCapture, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture y && y.Result == timestamp);
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
            var res = analyzer.GetCapture(isBeforeCapture, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture x && x.Result == timestamp);

            isBeforeCapture = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.IS_BEFORE)
                    {
                        Data = new List<string>()
                        {
                            DateTime.Now.ToString()
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };


            timestamp = DateTime.Now.AddDays(1);
            res = analyzer.GetCapture(isBeforeCapture, timestamp, null);
            Assert.IsTrue(res.Result?.Captures.First() is DateTimeCapture y && y.Result == timestamp);
        }

        [TestMethod]
        public void TestLTCapture()
        {
            var RuleName = "LT Capture";
            var analyzer = new Analyzer();
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

            Assert.IsTrue(analyzer.GetCapture(ltCaptureRule, 3, null).Result?.Captures.Any(x => x is IntCapture y && y.Result == 3) is true);

            ltCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.LT)
                    {
                        Data = new List<string>()
                        {
                            "5"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            Assert.IsTrue(analyzer.GetCapture(ltCaptureRule, 7, null).Result?.Captures.Any(x => x is IntCapture y && y.Result == 7) is true);
        }

        [TestMethod]
        public void TestWasModifiedCapture()
        {
            var RuleName = "WasModified Capture";
            var analyzer = new Analyzer();

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

            var testString = "The Secret is Magic";
            var testString2 = "The Secret is Science";
            var res = analyzer.GetCapture(wasModifiedRule, testString, testString2);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ComparisonResultCapture y && !y.Result.AreEqual) is true);

            wasModifiedRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.WAS_MODIFIED)
                    {
                        Capture = true,
                        Invert = true
                    }
                }
            };
            res = analyzer.GetCapture(wasModifiedRule, testString, testString);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ComparisonResultCapture y && y.Result.AreEqual) is true);
        }

        [TestMethod]
        public void TestRegexCapture()
        {
            var RuleName = "Regex Capture";
            var analyzer = new Analyzer();
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

            var testString = "The Secret is Magic";
            var res = analyzer.GetCapture(regexCaptureRule, testString, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is RegexCapture y && y.Result.Groups[1].Value == "Magic") is true);

            regexCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.REGEX)
                    {
                        Data = new List<string>()
                        {
                            "Secret.*([Mm]agic).*"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            testString = "The Secret is Tacos";
            res = analyzer.GetCapture(regexCaptureRule, testString, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is RegexCapture y) is true);
        }

        [TestMethod]
        public void TestGTCapture()
        {
            var RuleName = "GT Capture";
            var analyzer = new Analyzer();
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

            Assert.IsTrue(analyzer.GetCapture(gtCaptureRule, 7, null).Result?.Captures.Any(x => x is IntCapture y && y.Result == 7) is true);

            gtCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.GT)
                    {
                        Data = new List<string>()
                        {
                            "5"
                        },
                        Capture = true,
                        Invert = true
                    }
                }
            };

            Assert.IsTrue(analyzer.GetCapture(gtCaptureRule, 3, null).Result?.Captures.Any(x => x is IntCapture y && y.Result == 3) is true);
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

            containsCaptureRule = new Rule(RuleName)
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
                        Capture = true,
                        Invert = true
                    }
                }
            };

            res = analyzer.GetCapture(containsCaptureRule, "ThisStringHasNothing", null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is StringCapture y && y.Result == "ThisStringHasNothing") is true);

            res = analyzer.GetCapture(containsCaptureRule, new List<string>() { "None", "Null" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Contains("Null")) is true);

            res = analyzer.GetCapture(containsCaptureRule, Words.None, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is EnumCapture y && y.Result.HasFlag(Words.None)) is true);

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

            var testlist = testdata.ToList();

            res = analyzer.GetCapture(containsCaptureRule, testlist, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Version")) is true);

            containsCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS)
                    {
                        DictData = testlist,
                        Capture = true,
                        Invert = true
                    }
                }
            };
            testdata = new Dictionary<string, string>() { { "Something", "Else" }, { "Other", "Keys" } };

            res = analyzer.GetCapture(containsCaptureRule, testdata, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Something")) is true);

            testlist = testdata.ToList();

            res = analyzer.GetCapture(containsCaptureRule, testlist, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Other")) is true);
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

            containsAnyCaptureRule = new Rule(RuleName)
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
                        Capture = true,
                        Invert = true
                    }
                }
            };
            
            res = analyzer.GetCapture(containsAnyCaptureRule, "ThisStringIsn'tSpecial", null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is StringCapture y && y.Result == "ThisStringIsn'tSpecial") is true);

            res = analyzer.GetCapture(containsAnyCaptureRule, new List<string>() { "Pizza", "Taco" }, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListCapture<string> y && y.Result.Contains("Taco")) is true);

            res = analyzer.GetCapture(containsAnyCaptureRule, Words.None, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is EnumCapture y && y.Result.HasFlag(Words.None)) is true);

            testdata = new Dictionary<string, string>() { { "Version", "1.0" }, { "State", "Beta" } };

            containsAnyCaptureRule = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.CONTAINS_ANY)
                    {
                        DictData = testdata.ToList(),
                        Capture = true,
                        Invert = true
                    }
                }
            };

            testdata = new Dictionary<string, string>() { { "Not matching", "1.0" }, { "Status", "Beta" } };

            res = analyzer.GetCapture(containsAnyCaptureRule, testdata, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Status")) is true);

            testlist = testdata.ToList();

            res = analyzer.GetCapture(containsAnyCaptureRule, testlist, null);
            Assert.IsTrue(res.Result?.Captures.Any(x => x is ListKvpCapture<string, string> y && y.Result.Any(x => x.Key == "Status")) is true);
        }
    }
}
