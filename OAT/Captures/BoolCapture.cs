using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    public class BoolCapture : ClauseCapture
    {
        public BoolCapture(Clause clause, bool result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public bool Result { get; }
    }
}
