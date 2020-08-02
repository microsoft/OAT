using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    /// <summary>
    /// The class to hold arguments to pass to a clause
    /// </summary>
    public class OperationArguments
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="clause">The Clause this refers to</param>
        /// <param name="state1">Object state 1</param>
        /// <param name="state2">Object state 2</param>
        /// <param name="captures">The previous Clause Captures</param>
        public OperationArguments(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            this.clause = clause;
            this.state1 = state1;
            this.state2 = state2;
            this.captures = captures;
        }
        /// <summary>
        /// The clause associated with these arguments
        /// </summary>
        public Clause clause { get; }
        /// <summary>
        /// The first object state
        /// </summary>
        public object? state1 { get; }
        /// <summary>
        /// The second object state
        /// </summary>
        public object? state2 { get; }
        /// <summary>
        /// The captures from previous clauses in the same rule.
        /// </summary>
        public IEnumerable<ClauseCapture>? captures { get; }
    }
}
