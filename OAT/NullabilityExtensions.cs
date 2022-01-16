using System.Reflection;

namespace Microsoft.CST.OAT.Extensions
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// Provides IsNullable() extension methods using Reflection.
    /// </summary>
    public static class NullabilityExtensions
    {
        /// <summary>
        /// Determines if a property is nullable
        /// </summary>
        /// <param name="propertyInfo">The Property to check</param>
        /// <returns></returns>
        public static bool IsNullable(this PropertyInfo propertyInfo)
        {
            return nullabilityInfoContext.Create(propertyInfo).ReadState == NullabilityState.Nullable;
        }

        /// <summary>
        /// Determines if a field is nullable
        /// </summary>
        /// <param name="fieldInfo">The field</param>
        /// <returns></returns>
        public static bool IsNullable(this FieldInfo fieldInfo)
        {
            return nullabilityInfoContext.Create(fieldInfo).ReadState == NullabilityState.Nullable;
        }

        private static readonly NullabilityInfoContext nullabilityInfoContext = new();
    }
#endif
}
