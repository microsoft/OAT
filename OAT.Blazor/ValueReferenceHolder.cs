namespace Microsoft.CST.OAT.Blazor
{
    /// <summary>
    /// A holder to pass a value type by reference.
    /// </summary>
    /// <typeparam name="T">The Type of the value</typeparam>
    public class ValueReferenceHolder<T>
    {
        /// <summary>
        /// Create an instance of a holder to pass a value by reference.
        /// </summary>
        /// <param name="referenceToValue">The value to pass by reference.</param>
        public ValueReferenceHolder(ref T referenceToValue)
        {
            ReferenceToValue = referenceToValue;
        }

        public T ReferenceToValue { get; set; }
    }
}
