using Microsoft.CodeAnalysis.FlowAnalysis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.CST.OAT.Utils
{
    /// <summary>
    /// Some helper functions used by OAT.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Determines if a Regex is valid or not by checking if a Regex object can be created with it.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
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


        internal static object? GetValueByPropertyOrFieldNameInternal(object? obj, string? propertyName)
        {
            return obj?.GetType().GetProperty(propertyName ?? string.Empty)?.GetValue(obj) ?? obj?.GetType().GetField(propertyName ?? string.Empty)?.GetValue(obj);
        }

        /// <summary>
        ///     Gets the object value stored at the field or property named by the string. Property tried
        ///     first. Returns null if none found.
        /// </summary>
        /// <param name="obj"> The target object </param>
        /// <param name="propertyName"> The Property or Field name </param>
        /// <returns> The object at that Name or null </returns>
        public static object? GetValueByPropertyOrFieldName(object? obj, string? propertyName)
        {
            var obj2 = obj;
            foreach (var split in propertyName?.Split('.') ?? Array.Empty<string>())
            {
                obj2 = GetValueByPropertyOrFieldNameInternal(obj2, split);
            }
            return obj2;
        }

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

        /// <summary>
        /// Returns a list of the Types in the given namespace in the given assembly.
        /// </summary>
        /// <param name="assembly">The Assembly to scan</param>
        /// <param name="nameSpace">The Namespace to look for.</param>
        /// <returns></returns>
        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
}
}
