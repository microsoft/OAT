using System.Collections.Generic;

namespace Microsoft.CST.OAT
{
    public class DictCapture<T1,T2> : ClauseCapture
    {
        public DictCapture(Clause clause, Dictionary<T1,T2> result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public Dictionary<T1,T2> Result { get; }
    }
}
