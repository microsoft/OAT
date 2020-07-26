using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    public class OperationResult
    {
        public OperationResult(bool result, ClauseCapture? clauseCaptures)
        {
            Result = result;
            Capture = clauseCaptures;
        }

        public bool Result { get; }
        public ClauseCapture? Capture { get; }
    }
}
