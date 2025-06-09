using Microsoft.CST.OAT.Tests;
using Microsoft.CST.OAT.Utils;
using System;
using Xunit;

[assembly: AssemblyFixture(typeof(GlobalContext))]

namespace Microsoft.CST.OAT.Tests
{
    public class GlobalContext : IDisposable
    {
        public GlobalContext()
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }

        public void Dispose()
        {
        }
    }
}
