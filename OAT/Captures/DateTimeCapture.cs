using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    public class DateTimeCapture : ClauseCapture
    {
        public DateTimeCapture(Clause clause, DateTime result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public DateTime Result { get; }
    }
}
