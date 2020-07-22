using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CST.OAT
{
    /// <summary>
    /// Captures a List of Key Value Pairs
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class ListKvpCapture<T1,T2> : ClauseCapture
    {
        /// <summary>
        /// The constructor for a Capture of a List of KVP
        /// </summary>
        /// <param name="clause">The Clause</param>
        /// <param name="result">The List KVP</param>
        /// <param name="state1">The first object state</param>
        /// <param name="state2">The second object state</param>
        public ListKvpCapture(Clause clause, List<KeyValuePair<T1, T2>> result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        /// <summary>
        /// The List of KVP
        /// </summary>
        public List<KeyValuePair<T1,T2>> Result { get; }
    }
}
