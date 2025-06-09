using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.CST.OAT.Tests
{
    
    public class LambdaTests
    {
        [Fact]
        public void TestScriptingDisabled()
        {
            var okayLambda = @"return new OperationResult(state1 is true, null);";
            var invalidImportRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(okayLambda, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            // We should receive an issue that scripting is disabled
            Assert.True(analyzer
                .EnumerateRuleIssues(invalidImportRule)
                .Any());
        }

        [Fact]
        public void TestInvalidImports()
        {
            var okayLambda = @"return new OperationResult(state1 is true, null);";
            var invalidImportRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(okayLambda, new List<string>(){ "Not.A.Package" }, Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer(new AnalyzerOptions(true));
            Assert.True(analyzer
                .EnumerateRuleIssues(invalidImportRule)
                .Any());
        }

        [Fact]
        public void TestInvalidLambda()
        {
            var badLambda = @"This isn't valid code.";

            var badLambdaRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(badLambda, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer(new AnalyzerOptions(true));
            Assert.True(analyzer
                .EnumerateRuleIssues(badLambdaRule)
                .Any());
        }

        [Fact]
        public void TestInvalidReferences()
        {
            var okayLambda = @"return new OperationResult(state1 is true, null);";
            var invalidReferenceRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(okayLambda, Array.Empty<string>(), new List<string>(){ "Not.An.Assembly" })
                    }
                }
            };

            var analyzer = new Analyzer(new AnalyzerOptions(true));
            Assert.True(analyzer
                .EnumerateRuleIssues(invalidReferenceRule)
                .Any());
        }

        [Fact]
        public void TestMissingImport()
        {
            var missingReference = @"return new NotReferencedClass();";
            var missingReferenceRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(missingReference, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.True(analyzer
                .EnumerateRuleIssues(missingReferenceRule)
                .Any());
        }

        [Fact]
        public void TestMissingReference()
        {
            var missingReference = @"return new NotReferencedClass();";
            var missingReferenceRule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(missingReference, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer(new AnalyzerOptions(true));
            Assert.True(analyzer
                .EnumerateRuleIssues(missingReferenceRule)
                .Any());
        }

        [Fact]
        public void TestValidLambda()
        {
            var lambda = @"return new OperationResult(State1 is true, null);";

            var rule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Script)
                    {
                        Script = new ScriptData(lambda, Array.Empty<string>(), Array.Empty<string>())
                    }
                }
            };

            var analyzer = new Analyzer(new AnalyzerOptions(true));

            var ruleIssues = analyzer.EnumerateRuleIssues(rule).ToArray();
            Assert.False(ruleIssues.Any());

            var results = analyzer.Analyze(new Rule[] { rule }, true, true);
            Assert.True(results.Any());
        }
    }
}