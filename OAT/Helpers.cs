using Newtonsoft.Json;
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

        /// <summary>
        /// Checks if this is a basic type that OAT Blazor Supports
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBasicType(Type type)
        {
            if (type == typeof(string) || type == typeof(int) || type == typeof(char) || type == typeof(long) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(bool) ||
                type == typeof(uint) || type == typeof(ulong) || type == typeof(short) || type == typeof(ushort) ||
                type == typeof(DateTime) || type.IsEnum)
            {
                // Only return basic types
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get MemberInfo and Paths for all the BasicType properties and fields in the Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="currentPath"></param>
        /// <returns></returns>
        public static List<(string Path, MemberInfo MemInfo)> GetAllNestedFieldsAndPropertiesMemberInfo(Type type, string? currentPath = null)
        {
            var results = new List<(string Path, MemberInfo MemInfo)>();

            foreach (var property in type.GetProperties())
            {
                var newProperty = currentPath is null ? property.Name : $"{currentPath}.{property.Name}";
                if (IsBasicType(property.PropertyType))
                {
                    results.Add((newProperty, property));
                }
                else
                {
                    results.AddRange(GetAllNestedFieldsAndPropertiesMemberInfo(property.PropertyType, newProperty));
                }

            }
            foreach (var field in type.GetFields())
            {
                var newField = currentPath is null ? field.Name : $"{currentPath}.{field.Name}";
                if (IsBasicType(field.FieldType))
                {
                    results.Add((newField, field));
                }
                else
                {
                    results.AddRange(GetAllNestedFieldsAndPropertiesMemberInfo(field.FieldType, newField));
                }
            }
            return results;
        }

        /// <summary>
        /// Gets the Paths of all the Fields and Properties in the provided Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="currentPath"></param>
        /// <returns></returns>
        public static List<string> GetAllNestedFieldsAndProperties(Type type, string? currentPath = null)
        {
            var results = new List<string>();
            if (!string.IsNullOrEmpty(currentPath))
            {
                results.Add(currentPath);
            }
            if (type == typeof(string) || type == typeof(int) || type == typeof(char) || type == typeof(long) ||
                    type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(bool) ||
                    type == typeof(uint) || type == typeof(ulong) || type == typeof(short) || type == typeof(ushort) ||
                    type == typeof(DateTime) || type.IsEnum)
            {
                // Don't crawl into basic types
            }
            else
            {
                foreach (var property in type.GetProperties())
                {
                    var newProperty = currentPath is null ? property.Name : $"{currentPath}.{property.Name}";
                    results.AddRange(GetAllNestedFieldsAndProperties(property.PropertyType, newProperty));

                }
                foreach (var field in type.GetFields())
                {
                    var newField = currentPath is null ? field.Name : $"{currentPath}.{field.Name}";
                    results.AddRange(GetAllNestedFieldsAndProperties(field.FieldType, newField));
                }
            }
            return results;
        }

        internal static object? GetValueByPropertyOrFieldNameInternal(object? obj, string? propertyName)
        {
            if (obj is Dictionary<string,object> dict && propertyName != null)
            {
                return dict[propertyName];
            }
            else
            {
                return obj?.GetType().GetProperty(propertyName ?? string.Empty)?.GetValue(obj) ?? obj?.GetType().GetField(propertyName ?? string.Empty)?.GetValue(obj);
            }
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
            var obj2 = obj;
            var splits = propertyName?.Split('.');
            if (splits != null)
            {
                for (int i = 0; i < splits.Length - 1; i++)
                {
                    obj2 = GetValueByPropertyOrFieldNameInternal(obj2, splits[i]);
                }

                SetValueByPropertyOrFieldNameInternal(obj2, splits[^1], value);
            }
        }
        internal static void SetValueByPropertyOrFieldNameInternal(object? obj, string propertyName, object? value)
        {
            if (obj is Dictionary<string, object> dictionary)
            {
                if (value is { })
                {
                    dictionary[propertyName] = value;
                }
                return;
            }
            var prop = obj?.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                prop.SetValue(obj, value);
            }
            var field = obj?.GetType().GetField(propertyName);
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
            
            var types = new List<Type>();
            try
            {
                types.AddRange(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException e)
            {
                types.AddRange(e.Types.Where(x => x is Type));
                foreach (var ex in e.LoaderExceptions)
                {
                    Console.WriteLine($"Failed to load Type: {e.Message}");
                }
            }
            catch (Exception) { }
            return types.Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        /// <summary>
        /// Returns the assembly version string.
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            return (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false) as AssemblyInformationalVersionAttribute[])?[0].InformationalVersion ?? "Unknown";
        }
    }
}
