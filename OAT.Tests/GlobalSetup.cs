using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
[assembly: ClassCleanupExecution(ClassCleanupBehavior.EndOfClass)]

namespace Microsoft.CST.OAT.Tests
{
    [TestClass]
    public static class GlobalSetup
    {
        [AssemblyInitialize]
        public static void AssemblySetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }
    }
}
