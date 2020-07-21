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
    }
}
