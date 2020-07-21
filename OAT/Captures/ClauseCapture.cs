using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    public class ClauseCapture
    {
        public ClauseCapture(Clause clause, object? state1, object? state2)
        {
            Clause = clause;
            State1 = state1;
            State2 = state2;
        }

        public Clause Clause { get; }
        public object? State1 { get; }
        public object? State2 { get; }
    }
}
