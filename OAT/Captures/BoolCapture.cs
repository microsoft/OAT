namespace Microsoft.CST.OAT
{
    /// <summary>
    /// A ClauseCapture with a bool result
    /// </summary>
    public class BoolCapture : ClauseCapture
    {
        /// <summary>
        /// The constructor for a bool capture takes a bool in addition to normal ClauseCapture arguments
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="result"></param>
        /// <param name="state1"></param>
        /// <param name="state2"></param>
        public BoolCapture(Clause clause, bool result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        /// <summary>
        /// The Result of the Capture
        /// </summary>
        public bool Result { get; }
    }
}
