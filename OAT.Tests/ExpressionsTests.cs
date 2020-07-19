// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class AnalyzerTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Strings.Setup();
        }

        private TestObject testObjectTrueFalse = new TestObject()
        {
            StringField = "MagicWord",
            BoolField = false
        };

        private TestObject testObjectTrueTrue = new TestObject()
        {
            StringField = "MagicWord",
            BoolField = true
        };

        private TestObject testObjectFalseTrue = new TestObject()
        {
            StringField = "NotTheMagicWord",
            BoolField = true
        };

        private TestObject testObjectFalseFalse = new TestObject()
        {
            StringField = "NotTheMagicWord",
            BoolField = false
        };

        [TestMethod]
        public void TestNotNot()
        {
            var RuleName = "Not Not True";
            var notNotRule = new Rule(RuleName)
            {
                Expression = "NOT NOT 0",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { notNotRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void TestXorFromNand()
        {
            var RuleName = "XOR from NAND";
            var xorRule = new Rule(RuleName)
            {
                Expression = "(0 NAND (0 NAND 1)) NAND (1 NAND (0 NAND 1))",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause("BoolField", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { xorRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyAccessSubproperties()
        {
            var regObj = new TestObject()
            {
                StringDictField = new Dictionary<string, string>()
                {
                    { "One", "Two" }
                }
            };

            var RuleName = "ContainsRule";
            var containsRule = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringDictField.One", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "Two"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { containsRule };
            Assert.IsTrue(analyzer.Analyze(ruleList, regObj).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyAnd()
        {
            var RuleName = "AndRule";
            var andRule = new Rule(RuleName)
            {
                Expression = "0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause("BoolField", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { andRule };

            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyInvalidRuleDetection()
        {
            var invalidRule = new Rule("Unbalanced Parentheses")
            {
                Expression = "( 0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.EQ)
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };
            var analyzer = new Analyzer();
            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("ClauseInParenthesesLabel")
            {
                Expression = "WITH(PARENTHESIS)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "WITH(PARENTHESIS)"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersBetweenParentheses")
            {
                Expression = "(W(I",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "W(I"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersBeforeOpenParentheses")
            {
                Expression = "W(I",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "W(I"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersBetweenClosedParentheses")
            {
                Expression = "(0 AND W)I)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "W)I"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersAfterClosedParentheses")
            {
                Expression = "0 AND W)I",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "W)I"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CloseParenthesesWithNot")
            {
                Expression = "(0 AND NOT) 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "1"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("WhiteSpaceLabel")
            {
                Expression = "0 AND   ",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("InvalidOperator")
            {
                Expression = "0 XAND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "1"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("InvalidNotOperator")
            {
                Expression = "0 NOT AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "1"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("EndsWithOperator")
            {
                Expression = "0 AND",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("UnusedLabel")
            {
                Expression = "0",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "1"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("MissingLabel")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("ExpressionRequiresLabels")
            {
                Expression = "0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL),
                    new Clause("Path", OPERATION.IS_NULL)
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("OutOfOrder")
            {
                Expression = "0 1 AND",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("StartWithOperator")
            {
                Expression = "OR 0 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.EQ)
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("Case Sensitivity")
            {
                Expression = "Variable",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "VARIABLE"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("OPERATION.Custom without CustomOperation")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.CUSTOM)
                    {
                        Label = "VARIABLE"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("OPERATION.Custom without validation Delegate")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.CUSTOM)
                    {
                        Label = "VARIABLE",
                        CustomOperation = "NO_DELEGATE"
                    }
                }
            };

            Assert.IsFalse(analyzer.IsRuleValid(invalidRule));
        }

        [TestMethod]
        public void VerifyNand()
        {
            var RuleName = "NandRule";
            var nandRule = new Rule(RuleName)
            {
                Expression = "0 NAND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause("BoolField", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { nandRule };

            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyNor()
        {
            var RuleName = "NorRule";
            var norRule = new Rule(RuleName)
            {
                Expression = "0 NOR 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause("BoolField", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { norRule };

            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyCustom()
        {
            var RuleName = "CustomRule";
            var customRule = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.CUSTOM)
                    {
                        CustomOperation = "RETURN_TRUE",
                        Data = new List<string>()
                        {
                            "TestPath1"
                        }
                    },
                }
            };

            var analyzer = new Analyzer();

            analyzer.CustomOperationDelegates.Add((clause, listValues, dictionaryValues, before, after) =>
            {
                if (clause.Operation == OPERATION.CUSTOM)
                {
                    if (clause.CustomOperation == "RETURN_TRUE")
                    {
                        return (true,true);
                    }
                }
                return (false,false);
            });

            var ruleList = new List<Rule>() { customRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyNot()
        {
            var RuleName = "NotRule";
            var notRule = new Rule(RuleName)
            {
                Expression = "NOT 0",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { notRule };

            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyOr()
        {
            var RuleName = "OrRule";
            var orRule = new Rule(RuleName)
            {
                Expression = "0 OR 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause("BoolField", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { orRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.IsFalse(analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyValidRuleDetection()
        {
            var validRule = new Rule("Regular Rule")
            {
                Expression = "0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.EQ)
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer.IsRuleValid(validRule));

            validRule = new Rule("Extraneous Parenthesis")
            {
                Expression = "(0 AND 1)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.EQ)
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };

            Assert.IsTrue(analyzer.IsRuleValid(validRule));

            validRule = new Rule("Deeply Nested Expression")
            {
                Expression = "(0 AND 1) OR (2 XOR (3 AND (4 NAND 5)) OR 6)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.EQ)
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "2"
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "3"
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "4"
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "5"
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "6"
                    }
                }
            };

            Assert.IsTrue(analyzer.IsRuleValid(validRule));

            validRule = new Rule("StringsForClauseLabels")
            {
                Expression = "FOO AND BAR OR BA$_*",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "FOO"
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "BAR"
                    },
                    new Clause("IsExecutable", OPERATION.IS_NULL)
                    {
                        Label = "BA$_*"
                    }
                }
            };

            Assert.IsTrue(analyzer.IsRuleValid(validRule));

            validRule = new Rule("MultipleConsecutiveNots")
            {
                Expression = "0 AND NOT NOT 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "1"
                    },
                    new Clause("Path", OPERATION.IS_NULL)
                    {
                        Label = "0"
                    }
                }
            };

            Assert.IsTrue(analyzer.IsRuleValid(validRule));
        }

        [TestMethod]
        public void VerifyBareObjectQuery()
        {
            var RuleName = "BareObjectRule";
            var bareObjectRule = new Rule(RuleName)
            {
                Target = "string",
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.EQ)
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    }
                }
            };

            var bareObjectRuleNoTarget = new Rule(RuleName)
            {
                Clauses = new List<Clause>()
                {
                    new Clause(OPERATION.EQ)
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { bareObjectRule, bareObjectRuleNoTarget };

            Assert.IsTrue(analyzer.Analyze(ruleList, "MagicWord").Count() == 2);
        }

        [TestMethod]
        public void VerifyXor()
        {
            var RuleName = "XorRule";
            var xorRule = new Rule(RuleName)
            {
                Expression = "0 XOR 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("StringField", OPERATION.EQ)
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause("BoolField", OPERATION.IS_TRUE)
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { xorRule };

            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.IsTrue(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.IsTrue(analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
        }

        [TestMethod]
        public void VerifyCustomRuleValidation()
        {
            var RuleName = "CustomRuleValidation";
            var supportedCustomOperation = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.CUSTOM)
                    {
                        CustomOperation = "FOO",
                        Data = new List<string>()
                        {
                            "Some Data"
                        }
                    },
                }
            };

            var unsupportedCustomOperation = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause("Path", OPERATION.CUSTOM)
                    {
                        CustomOperation = "BAR",
                        Data = new List<string>()
                        {
                            "Some Data"
                        }
                    },
                }
            };


            var analyzer = new Analyzer();

            // Rules aren't valid without a validation delegate
            Assert.IsFalse(analyzer.IsRuleValid(supportedCustomOperation));

            analyzer.CustomOperationValidationDelegates.Add(parseFooOperations);

            (bool Applies, IEnumerable<Violation> FoundViolations) parseFooOperations(Rule r, Clause c)
            {
                switch (c.CustomOperation)
                {
                    case "FOO":
                        var violations = new List<Violation>();

                        if (!c.Data.Any())
                        {
                            violations.Add(new Violation("FOO Operation expects data", r, c));
                        }

                        return (true, violations);
                    default:
                        return (false, Array.Empty<Violation>());
                }
            };

            Assert.IsTrue(analyzer.IsRuleValid(supportedCustomOperation));
            Assert.IsFalse(analyzer.IsRuleValid(unsupportedCustomOperation));
        }
    }
}