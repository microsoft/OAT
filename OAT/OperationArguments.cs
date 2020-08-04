using System.Collections.Generic;

namespace Microsoft.CST.OAT
{
    /// <summary>
    ///     The class to hold arguments to pass to a clause
    /// </summary>
    public class OperationArguments
    {
        /// <summary>
        ///     The constructor
        /// </summary>
        /// <param name="clause"> The Clause this refers to </param>
        /// <param name="state1"> Object state 1 </param>
        /// <param name="state2"> Object state 2 </param>
        /// <param name="captures"> The previous Clause Captures </param>
        public OperationArguments(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            Clause = clause;
            State1 = state1;
            State2 = state2;
            Captures = captures;
        }

        /// <summary>
        ///     The captures from previous clauses in the same rule.
        /// </summary>
        public IEnumerable<ClauseCapture>? Captures { get; }

        /// <summary>
        ///     The clause associated with these arguments
        /// </summary>
        public Clause Clause { get; }

        /// <summary>
        ///     The first object state
        /// </summary>
        public object? State1 { get; }

        /// <summary>
        ///     The second object state
        /// </summary>
        public object? State2 { get; }
    }
}