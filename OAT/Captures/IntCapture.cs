namespace Microsoft.CST.OAT
{
    public class IntCapture : ClauseCapture
    {
        public IntCapture(Clause clause, int result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public int Result { get; }
    }
}
