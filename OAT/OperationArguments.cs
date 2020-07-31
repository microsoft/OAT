using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    public class OperationArguments
    {
        public OperationArguments(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            this.clause = clause;
            this.state1 = state1;
            this.state2 = state2;
            this.captures = captures;
        }
        public Clause clause { get; }
        public object? state1 { get; }
        public object? state2 { get; }
        public IEnumerable<ClauseCapture>? captures { get; }
    }
}
