using System.Collections.Generic;

namespace Microsoft.CST.OAT
{
    public class ListCapture<T> : ClauseCapture
    {
        public ListCapture(Clause clause, List<T> result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public List<T> Result { get; }
    }
}
