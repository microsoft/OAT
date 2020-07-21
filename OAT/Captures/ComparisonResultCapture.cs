using KellermanSoftware.CompareNetObjects;

namespace Microsoft.CST.OAT
{
    public class ComparisonResultCapture : ClauseCapture
    {
        public ComparisonResultCapture(Clause clause, ComparisonResult result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public ComparisonResult Result { get; }
    }
}
