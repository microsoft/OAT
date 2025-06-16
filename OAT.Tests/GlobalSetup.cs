using Microsoft.CST.OAT.Tests;
using Microsoft.CST.OAT.Utils;
using System;
using Xunit;

namespace Microsoft.CST.OAT.Tests
{
    public class GlobalContext
    {
        public GlobalContext()
        {
            Logger.SetupVerbose();
            Strings.Setup();
        }
    }
}
