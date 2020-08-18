using Serilog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.CST.OAT.Utils
{
    public static class Helpers
    {
        public static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Gets the object value stored at the field or property named by the string. Property tried
        ///     first. Returns null if none found.
        /// </summary>
        /// <param name="obj"> The target object </param>
        /// <param name="propertyName"> The Property or Field name </param>
        /// <returns> The object at that Name or null </returns>
        public static object? GetValueByPropertyOrFieldName(object? obj, string? propertyName) => obj?.GetType().GetProperty(propertyName ?? string.Empty)?.GetValue(obj) ?? obj?.GetType().GetField(propertyName ?? string.Empty)?.GetValue(obj);

        /// <summary>
        ///     Sets the object value stored at the field or property named by the string. Property tried
        ///     first.
        /// </summary>
        /// <param name="obj"> The target object </param>
        /// <param name="propertyName"> The Property or Field name </param>
        /// <param name="value">The value to set.</param>
        /// <returns> The object at that Name or null </returns>
        public static void SetValueByPropertyOrFieldName(object? obj, string? propertyName, object? value)
        {
            var prop = obj?.GetType().GetProperty(propertyName ?? string.Empty);
            if (prop != null)
            {
                prop.SetValue(obj, value);
            }
            var field = obj?.GetType().GetField(propertyName ?? string.Empty);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        /// <summary>
        ///     Prints out the Enumerable of violations to Warning
        /// </summary>
        /// <param name="violations"> An Enumerable of Violations to print </param>
        public static void PrintViolations(IEnumerable<Violation> violations)
        {
            if (violations == null) return;
            foreach (var violation in violations)
            {
                Log.Warning(violation.Description);
            }
        }
    }
}
