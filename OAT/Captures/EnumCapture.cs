using System;

namespace Microsoft.CST.OAT
{
    public class EnumCapture : ClauseCapture
    {
        public EnumCapture(Clause clause, Enum result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public Enum Result { get; }
    }
}
