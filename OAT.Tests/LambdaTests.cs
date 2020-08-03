﻿using KellermanSoftware.CompareNetObjects;
using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public class LambdaTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        [TestMethod]
        public void TestScriptingDisabled()
        {
            var okayLambda = @"return new OperationResult(state1 is true, null);";
            var invalidImportRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(okayLambda, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsFalse(analyzer
                .EnumerateRuleIssues(invalidImportRule)
                .Any());
        }

        [TestMethod]
        public void TestInvalidImports()
        {
            var okayLambda = @"return new OperationResult(state1 is true, null);";
            var invalidImportRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(okayLambda, new List<string>(){ "Not.A.Package" }, Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            analyzer.RunScripts = true;
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(invalidImportRule)
                .Any());
        }

        [TestMethod]
        public void TestInvalidLambda()
        {
            var badLambda = @"This isn't valid code.";

            var badLambdaRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(badLambda, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            analyzer.RunScripts = true;
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(badLambdaRule)
                .Any());
        }

        [TestMethod]
        public void TestInvalidReferences()
        {
            var okayLambda = @"return new OperationResult(state1 is true, null);";
            var invalidReferenceRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(okayLambda, Array.Empty<string>(), new List<string>(){ "Not.An.Assembly" })
                    }
                }
            };

            var analyzer = new Analyzer();
            analyzer.RunScripts = true;
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(invalidReferenceRule)
                .Any());
        }

        [TestMethod]
        public void TestMissingImport()
        {
            var missingReference = @"return new NotReferencedClass();";
            var missingReferenceRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(missingReference, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(missingReferenceRule)
                .Any());
        }

        [TestMethod]
        public void TestMissingReference()
        {
            var missingReference = @"return new NotReferencedClass();";
            var missingReferenceRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(missingReference, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            analyzer.RunScripts = true;
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(missingReferenceRule)
                .Any());
        }

        [TestMethod]
        public void TestValidLambda()
        {
            var lambda = @"return new OperationResult(State1 is true, null);";

            var rule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Script = new ScriptData(lambda, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            analyzer.RunScripts = true;
            var ruleIssues = analyzer.EnumerateRuleIssues(rule).ToArray();
            Assert.IsFalse(ruleIssues.Any());
            var results = analyzer.Analyze(new Rule[] { rule }, true, true);
            Assert.IsTrue(results.Any());
        }
    }
}