using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    class LambdaTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        [TestMethod]
        public void TestValidLambda()
        {
            var lambda = @"return new OperationResult(state1 is true, null);";

            var rule = new Rule("Lambda Rule")
            {
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Custom)
                    {
                        Lambda = lambda
                    }
                }
            };

            var analyzer = new Analyzer();
            var results = analyzer.Analyze(new Rule[] { rule }, true, true);
            Assert.IsTrue(results.Any());
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
                        Lambda = badLambda
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(badLambdaRule)
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
                        Lambda = okayLambda,
                        Imports = new List<string>(){ "Not.A.Package" }
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(invalidImportRule)
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
                        Lambda = okayLambda,
                        References = new List<string>(){ "Not.An.Assembly" }
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(invalidReferenceRule)
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
                        Lambda = missingReference
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(missingReferenceRule)
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
                        Lambda = missingReference
                    }
                }
            };

            var analyzer = new Analyzer();
            Assert.IsTrue(analyzer
                .EnumerateRuleIssues(missingReferenceRule)
                .Any());
        }
    }
}
