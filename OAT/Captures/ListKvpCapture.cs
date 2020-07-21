using System.Collections.Generic;

namespace Microsoft.CST.OAT
{
    public class ListKvpCapture<T1,T2> : ClauseCapture
    {
        public ListKvpCapture(Clause clause, List<KeyValuePair<T1, T2>> result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public List<KeyValuePair<T1,T2>> Result { get; }
    }
}
