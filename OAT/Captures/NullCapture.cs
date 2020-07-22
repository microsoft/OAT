namespace Microsoft.CST.OAT
{
    /// <summary>
    /// Holds a clause and object states, can be extended to hold a Result with specific data
    /// </summary>
    public class NullCapture : ClauseCapture
    {
        /// <summary>
        /// A basic Clause Capture constructor
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="state1"></param>
        /// <param name="state2"></param>
        public NullCapture(Clause clause, object? state1, object? state2) : base (clause,state1,state2)
        {
        }
        
        /// <summary>
        /// Always null
        /// </summary>
        public object? Result { get { return null; } }
    }
}
