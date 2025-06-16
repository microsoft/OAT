// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using Microsoft.CST.OAT.Operations;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.CST.OAT.Tests
{
    
    public class AnalyzerTests
    {
        private readonly TestObject testObjectTrueFalse = new TestObject()
        {
            StringField = "MagicWord",
            BoolField = false
        };

        private readonly TestObject testObjectTrueTrue = new TestObject()
        {
            StringField = "MagicWord",
            BoolField = true
        };

        private readonly TestObject testObjectFalseTrue = new TestObject()
        {
            StringField = "NotTheMagicWord",
            BoolField = true
        };

        private readonly TestObject testObjectFalseFalse = new TestObject()
        {
            StringField = "NotTheMagicWord",
            BoolField = false
        };

        [Fact]
        public void TestShortcutWithCapture()
        {
            var rule = new Rule("TestShortcutWithCapture")
            {
                Expression = "1 AND 2 OR 3",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsTrue,"BoolField"){
                        Label = "1",
                        Invert = true
                    },
                    new Clause(Operation.Equals,"StringField"){
                        Label = "2",
                        Data = new List<string>()
                        {
                            "Magic"
                        },
                        Capture = true
                    },
                    new Clause(Operation.IsTrue,"BoolField"){
                        Label = "3"
                    }
                }
            };

            var analyzer = new Analyzer();
            var target = new TestObject()
            {
                StringField = "Magic",
                BoolField = true
            };
            var cap = analyzer.GetCapture(rule, target);
            Assert.True(cap.Result?.Captures.First() is TypedClauseCapture<string> t && t.Result == "Magic");
        }

        [Fact]
        public void TestShortcut()
        {
            var rule = new Rule("TestShortcut")
            {
                Expression = "1 AND 2 OR 3",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsTrue,"BoolField"){
                        Label = "1",
                        Invert = true
                    },
                    new Clause(Operation.Custom,"StringField"){
                        Label = "2"
                    },
                    new Clause(Operation.IsTrue,"BoolField"){
                        Label = "3"
                    }
                }
            };

            var rule2 = new Rule("TestShortcut")
            {
                Expression = "1 OR 2",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsTrue,"BoolField"){
                        Label = "1",
                    },
                    new Clause(Operation.Custom,"StringField"){
                        CustomOperation = "BAR",
                        Label = "2",
                    }
                }
            };

            var analyzer = new Analyzer();

            analyzer.SetOperation(new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "BAR",
                OperationDelegate = (Clause _, object? __, object? ___, IEnumerable<ClauseCapture>? c) =>
                {
                    // We should shortcut calling the custom operation entirely, because it is not being captured
                    // Given the test data this line should never be hit
                    Assert.Fail();
                    return new OperationResult(false, null);
                }
            });
            var target = new TestObject()
            {
                StringField = "Magic",
                BoolField = true
            };
            var cap = analyzer.GetCapture(rule, target);
            cap = analyzer.GetCapture(rule2, target);
        }

        [Fact]
        public void TestNotNot()
        {
            var RuleName = "Not Not True";
            var notNotRule = new Rule(RuleName)
            {
                Expression = "NOT NOT 0",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
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

            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
        }

        [Fact]
        public void TestXorFromNand()
        {
            var RuleName = "XOR from NAND";
            var xorRule = new Rule(RuleName)
            {
                Expression = "(0 NAND (0 NAND 1)) NAND (1 NAND (0 NAND 1))",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { xorRule };

            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);
            Assert.True(analyzer.Analyze(ruleList, testObjectTrueTrue).All(x => x.Name != RuleName));
            Assert.True(analyzer.Analyze(ruleList, testObjectFalseFalse).All(x => x.Name != RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName);
        }

        [Fact]
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
                    new Clause(Operation.Equals, "StringDictField.One")
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
            Assert.Contains(analyzer.Analyze(ruleList, regObj), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyImplicitAndWithInvert()
        {
            var RuleName = "ImplicitAndWithInvert";

            var implicitAndWithInvert = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        },
                        Invert = true
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { implicitAndWithInvert };

            Assert.True(analyzer.Analyze(ruleList, testObjectFalseFalse).All(x => x.Name != RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName); // The first clause is inverted
            Assert.True(analyzer.Analyze(ruleList, testObjectTrueTrue).All(x => x.Name != RuleName));
            Assert.True(analyzer.Analyze(ruleList, testObjectTrueFalse).All(x => x.Name != RuleName));
        }

        [Fact]
        public void VerifyImplicitAnd()
        {
            var RuleName = "ImplicitAnd";

            var implicitAnd = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { implicitAnd };

            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
        }

        [Fact]
        public void VerifyImplicitClauseLabels()
        {
            var RuleName = "ImplicitClauseLabels";

            var implicitClauseLabels = new Rule(RuleName)
            {
                Target = "TestObject",
                Expression = "0 OR 1",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { implicitClauseLabels };

            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);

            var mixedClauseLabels = new Rule(RuleName)
            {
                Target = "TestObject",
                Expression = "0 OR Label",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "Label"
                    }
                }
            };

            ruleList = new List<Rule>() { mixedClauseLabels };

            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyAnd()
        {
            var RuleName = "AndRule";
            var andRule = new Rule(RuleName)
            {
                Expression = "0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { andRule };

            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
        }

        [Fact]
        public void VerifyInvalidRuleDetection()
        {
            var invalidRule = new Rule("Unbalanced Parentheses")
            {
                Expression = "( 0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause(Operation.Equals, "IsExecutable")
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
            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("ClauseInParenthesesLabel")
            {
                Expression = "WITH(PARENTHESIS)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "WITH(PARENTHESIS)"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersBetweenParentheses")
            {
                Expression = "(W(I",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "W(I"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersBeforeOpenParentheses")
            {
                Expression = "W(I",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "W(I"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersBetweenClosedParentheses")
            {
                Expression = "(0 AND W)I)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "W)I"
                    },
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CharactersAfterClosedParentheses")
            {
                Expression = "0 AND W)I",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "W)I"
                    },
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("CloseParenthesesWithNot")
            {
                Expression = "(0 AND NOT) 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "1"
                    },
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("WhiteSpaceLabel")
            {
                Expression = "0 AND   ",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("InvalidOperator")
            {
                Expression = "0 XAND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "1"
                    },
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("InvalidNotOperator")
            {
                Expression = "0 NOT AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "1"
                    },
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("EndsWithOperator")
            {
                Expression = "0 AND",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("OutOfOrder")
            {
                Expression = "0 1 AND",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause(Operation.IsTrue, "IsExecutable")
                    {
                        Label = "1"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("StartWithOperator")
            {
                Expression = "OR 0 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause(Operation.Equals, "IsExecutable")
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("Case Sensitivity")
            {
                Expression = "Variable",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "VARIABLE"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("OPERATION.Custom without CustomOperation")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom, "Path")
                    {
                        Label = "VARIABLE"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("OPERATION.Custom without validation Delegate")
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom, "Path")
                    {
                        Label = "VARIABLE",
                        CustomOperation = "NO_DELEGATE"
                    }
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));

            invalidRule = new Rule("Missing Clause Labels")
            {
                Target = "TestObject",
                Expression = "1",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                }
            };

            Assert.False(analyzer.IsRuleValid(invalidRule));
        }

        [Fact]
        public void VerifyNand()
        {
            var RuleName = "NandRule";
            var nandRule = new Rule(RuleName)
            {
                Expression = "0 NAND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { nandRule };

            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseFalse), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyNor()
        {
            var RuleName = "NorRule";
            var norRule = new Rule(RuleName)
            {
                Expression = "0 NOR 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { norRule };

            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseTrue).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseFalse), x => x.Name == RuleName);
            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueFalse).Any(x => x.Name == RuleName));
            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
        }

        [Fact]
        public void VerifyCustom()
        {
            var RuleName = "CustomRule";
            var customRule = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom, "StringField")
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

            analyzer.SetOperation(new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "RETURN_TRUE",
                OperationDelegate = (_, __, ___, ____) => new OperationResult(true, null)
            });

            var ruleList = new List<Rule>() { customRule };

            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyCustomImplicitAndWithCaptures()
        {
            var RuleName = "CustomRule";
            var customRule = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Regex, "StringField")
                    {
                        Label = "Regex",
                        Data = new List<string>()
                        {
                            "Magic"
                        },
                        Capture = true
                    },
                    new Clause(Operation.Custom, "StringField")
                    {
                        CustomOperation = "DOUBLE_CHECK",
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    },
                }
            };

            var analyzer = new Analyzer();

            analyzer.SetOperation(new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "DOUBLE_CHECK",
                OperationDelegate = (clause, before, after, captures) =>
                {
                    if (captures != null)
                    {
                        var regexCapture = captures.Where(x => x.Clause.Label == "Regex").FirstOrDefault();
                        if (regexCapture is TypedClauseCapture<List<Match>> tcc)
                        {
                            if (tcc.Result[0].Groups[0].Value == "Magic")
                            {
                                return new OperationResult(true, null);
                            }
                        }
                    }
                    return new OperationResult(false, null);
                }
            });

            var ruleList = new List<Rule>() { customRule };

            Assert.True(analyzer.GetCaptures(ruleList, testObjectTrueTrue).Any());
        }

        [Fact]
        public void VerifyCustomExpressionWithCaptures()
        {
            var RuleName = "CustomRule";
            var customRule = new Rule(RuleName)
            {
                Target = "TestObject",
                Expression = "0 AND 1",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Regex, "StringField")
                    {
                        Label = "Regex",
                        Data = new List<string>()
                        {
                            "Magic"
                        },
                        Capture = true
                    },
                    new Clause(Operation.Custom, "StringField")
                    {
                        CustomOperation = "DOUBLE_CHECK",
                        Data = new List<string>()
                        {
                            "Magic"
                        }
                    },
                }
            };

            var analyzer = new Analyzer();

            analyzer.SetOperation(new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "DOUBLE_CHECK",
                OperationDelegate = (clause, before, after, captures) =>
                {
                    if (captures != null)
                    {
                        var regexCapture = captures.Where(x => x.Clause.Label == "Regex").FirstOrDefault();
                        if (regexCapture is TypedClauseCapture<List<Match>> tcc)
                        {
                            if (tcc.Result[0].Groups[0].Value == clause.Data?[0])
                            {
                                return new OperationResult(true, null);
                            }
                        }
                    }
                    return new OperationResult(false, null);
                }
            });

            var ruleList = new List<Rule>() { customRule };

            Assert.True(analyzer.GetCaptures(ruleList, testObjectTrueTrue).Any());
        }

        [Fact]
        public void VerifyNot()
        {
            var RuleName = "NotRule";
            var notRule = new Rule(RuleName)
            {
                Expression = "NOT 0",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
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

            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseFalse), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyOr()
        {
            var RuleName = "OrRule";
            var orRule = new Rule(RuleName)
            {
                Expression = "0 OR 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { orRule };

            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueTrue), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName);
            Assert.DoesNotContain(analyzer.Analyze(ruleList, testObjectFalseFalse), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyValidRuleDetection()
        {
            var validRule = new Rule("Regular Rule")
            {
                Expression = "0 AND 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause(Operation.Equals, "IsExecutable")
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
            Assert.True(analyzer.IsRuleValid(validRule));

            validRule = new Rule("Extraneous Parenthesis")
            {
                Expression = "(0 AND 1)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause(Operation.Equals, "IsExecutable")
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    }
                }
            };

            Assert.True(analyzer.IsRuleValid(validRule));

            validRule = new Rule("Deeply Nested Expression")
            {
                Expression = "(0 AND 1) OR (2 XOR (3 AND (4 NAND 5)) OR 6)",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "Path")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "TestPath2"
                        }
                    },
                    new Clause(Operation.Equals, "IsExecutable")
                    {
                        Label = "1",
                        Data = new List<string>()
                        {
                            "True"
                        }
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "2"
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "3"
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "4"
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "5"
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "6"
                    }
                }
            };

            Assert.True(analyzer.IsRuleValid(validRule));

            validRule = new Rule("StringsForClauseLabels")
            {
                Expression = "FOO AND BAR OR BA$_*",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "FOO"
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "BAR"
                    },
                    new Clause(Operation.IsNull, "IsExecutable")
                    {
                        Label = "BA$_*"
                    }
                }
            };

            Assert.True(analyzer.IsRuleValid(validRule));

            validRule = new Rule("MultipleConsecutiveNots")
            {
                Expression = "0 AND NOT NOT 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "1"
                    },
                    new Clause(Operation.IsNull, "Path")
                    {
                        Label = "0"
                    }
                }
            };

            Assert.True(analyzer.IsRuleValid(validRule));
        }

        [Fact]
        public void VerifyBareObjectQuery()
        {
            var RuleName = "BareObjectRule";
            var bareObjectRule = new Rule(RuleName)
            {
                Target = "string",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals)
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
                    new Clause(Operation.Equals)
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

            Assert.True(analyzer.Analyze(ruleList, "MagicWord").Count() == 2);
        }

        [Fact]
        public void VerifyXor()
        {
            var RuleName = "XorRule";
            var xorRule = new Rule(RuleName)
            {
                Expression = "0 XOR 1",
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "StringField")
                    {
                        Label = "0",
                        Data = new List<string>()
                        {
                            "MagicWord"
                        }
                    },
                    new Clause(Operation.IsTrue, "BoolField")
                    {
                        Label = "1"
                    }
                }
            };

            var analyzer = new Analyzer();
            var ruleList = new List<Rule>() { xorRule };

            Assert.Contains(analyzer.Analyze(ruleList, testObjectTrueFalse), x => x.Name == RuleName);
            Assert.True(!analyzer.Analyze(ruleList, testObjectTrueTrue).Any(x => x.Name == RuleName));
            Assert.True(!analyzer.Analyze(ruleList, testObjectFalseFalse).Any(x => x.Name == RuleName));
            Assert.Contains(analyzer.Analyze(ruleList, testObjectFalseTrue), x => x.Name == RuleName);
        }

        [Fact]
        public void VerifyCustomRuleValidation()
        {
            var RuleName = "CustomRuleValidation";
            var supportedCustomOperation = new Rule(RuleName)
            {
                Target = "TestObject",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom, "Path")
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
                    new Clause(Operation.Custom, "Path")
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
            Assert.False(analyzer.IsRuleValid(supportedCustomOperation));

            IEnumerable<Violation> validationDelegate(Rule r, Clause c)
            {
                if (!c.Data.Any())
                {
                    yield return new Violation("FOO Operation expects data", r, c);
                }
            }

            var fooOperation = new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "FOO",
                ValidationDelegate = validationDelegate
            };

            analyzer.SetOperation(fooOperation);

            Assert.True(analyzer.IsRuleValid(supportedCustomOperation));
            Assert.False(analyzer.IsRuleValid(unsupportedCustomOperation));
        }
    }
}