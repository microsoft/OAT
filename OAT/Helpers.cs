using Microsoft.CodeAnalysis.Diagnostics;
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
        /// Determines if a type is nullable
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns></returns>
        public static bool IsNullable(Type? type)
        {
            if (type == null) return true; // obvious
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
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
            if (!string.IsNullOrEmpty(currentPath) && currentPath != null)
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

        /// <summary>
        /// Recursively checks if the object Type given can be constructed of basic types or of those found in the extra assemblies provided.
        /// </summary>
        /// <param name="type">The Type to Check</param>
        /// <param name="extraAssemblies">Any asesemblies to use other than basic types.</param>
        /// <returns></returns>
        public static bool ConstructedOfLoadedTypes(Type type, IEnumerable<Assembly>? extraAssemblies = null)
        {            
            foreach(var ctor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public))
            {
                if (ConstructedOfLoadedTypes(ctor, extraAssemblies))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the ConstructorInfo given is constructable
        /// </summary>
        /// <param name="constructorInfo"></param>
        /// <param name="extraAssemblies">A list of assemblies that will be available to construct with.</param>
        /// <returns>true if only basic types and types derived from basic types can be used to construct.</returns>
        public static bool ConstructedOfLoadedTypes(ConstructorInfo constructorInfo, IEnumerable<Assembly>? extraAssemblies = null)
        {
            foreach (var parameter in constructorInfo.GetParameters())
            {
                if (IsBasicType(parameter.ParameterType)){
                    continue;
                }
                else
                {
                    if (constructorInfo.DeclaringType?.Assembly.GetTypes().Contains(parameter.ParameterType) ?? false)
                    {
                        continue;
                    }
                    
                    if (extraAssemblies != null)
                    {
                        if (extraAssemblies.Where(x => x.GetTypes().Contains(parameter.ParameterType)).FirstOrDefault() != null)
                        {
                            if (ConstructedOfLoadedTypes(parameter.ParameterType, extraAssemblies))
                            {
                                continue;
                            }
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if the ConstructorInfo given is constructable from basic types
        /// </summary>
        /// <param name="constructorInfo"></param>
        /// <returns>true if only basic types and types derived from basic types can be used to construct.</returns>
        public static bool ConstructedOfBasicTypes(ConstructorInfo constructorInfo)
        {
            return ConstructedOfLoadedTypes(constructorInfo);
        }

        internal static object? GetValueByPropertyOrFieldNameInternal(object? obj, string? propertyName)
        {
            if (propertyName is null) { return null; }
            if (obj is System.Collections.IDictionary dict)
            {
                if (dict.Keys.OfType<string>().Any(x => x == propertyName))
                {
                    if (dict[propertyName] is System.ValueTuple<object, Type> tuple)
                    {
                        return tuple.Item1;
                    }
                    else
                        return dict[propertyName];
                }
            }
            else if (obj is System.Collections.IList list)
            {
                if (int.TryParse(propertyName, out var propertyIndex) && list.Count > propertyIndex)
                {
                    return list[propertyIndex];
                }
            }
            else
            {
                return obj?.GetType().GetProperty(propertyName)?.GetValue(obj) ?? obj?.GetType().GetField(propertyName)?.GetValue(obj);
            }
            return null;
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
        /// Gets a sensible default value for the type.
        /// </summary>
        /// <param name="type">The type to get a default value for.</param>
        /// <returns>The default value for the type.</returns>
        public static object? GetDefaultValueForType(Type? type)
        {
            if (type is null)
            {
                return null;
            }
            else if (type.Equals(typeof(string)))
            {
                return string.Empty;
            }
            else if (type.Equals(typeof(int)))
            {
                return 0;
            }
            else if (type == typeof(char))
            {
                return ' ';
            }
            else if (type == typeof(long))
            {
                return (long)0;
            }
            else if (type == typeof(float))
            {
                return (float)0;
            }
            else if (type == typeof(double))
            {
                return (double)0;
            }
            else if (type == typeof(decimal))
            {
                return (decimal)0;
            }
            else if (type == typeof(bool))
            {
                return false;
            }
            else if (type == typeof(uint))
            {
                return (uint)0;
            }
            else if (type == typeof(ulong))
            {
                return (ulong)0;
            }
            else if (type == typeof(short))
            {
                return (short)0;
            }
            else if (type == typeof(ushort))
            {
                return (ushort)0;
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.MinValue;
            }
            else if (type.IsEnum)
            {
                return GetDefaultValueForType(type.GetEnumUnderlyingType());
            }
            else if (type == typeof(List<string>))
            {
                return new List<string>();
            }
            else if (type == typeof(List<KeyValuePair<string,string>>))
            {
                return new List<KeyValuePair<string, string>>();
            }
            else if (type == typeof(Dictionary<string, List<string>>))
            {
                return new Dictionary<string, List<string>>();
            }
            else if (type == typeof(Dictionary<string, string>))
            {
                return new Dictionary<string, string>();
            }
            else
            {
                return null;
            }
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
                for (var i = 0; i < splits.Length - 1; i++)
                {
                    obj2 = GetValueByPropertyOrFieldNameInternal(obj2, splits[i]);
                }
                SetValueByPropertyOrFieldNameInternal(obj2, splits[^1], value);
            }
        }
        internal static void SetValueByPropertyOrFieldNameInternal(object? obj, string propertyName, object? value)
        {
            if (obj is IDictionary<string, (object, Type)> dictionary)
            {
                var type = ((ValueTuple<object, Type>)dictionary[propertyName]).Item2;
                dictionary[propertyName] = (value!, type);
            }
            else if (obj is IList<object> list && int.TryParse(propertyName, out var propertyIndex) && list.Count > propertyIndex)
            {
                list[propertyIndex] = value!;
            }
            else
            {
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
