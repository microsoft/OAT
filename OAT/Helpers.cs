using Serilog;
using System;
using System.Collections;
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
        /// Create a list to hold the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>A List of the given type or null.</returns>
        public static IList? CreateList(Type? type)
        {
            if (type is null)
            {
                return null;
            }
            Type genericListType = typeof(List<>).MakeGenericType(type);
            return (IList?)Activator.CreateInstance(genericListType);
        }

        /// <summary>
        /// Create a Dictionary to hold the given types
        /// </summary>
        /// <param name="keyType">The Type of the Key</param>
        /// <param name="valueType">The Type of the Value</param>
        /// <returns>A Dictionary of the given types or null.</returns>
        public static IDictionary? CreateDictionary(Type? keyType, Type? valueType)
        {
            if (keyType is null || valueType is null)
            {
                return null;
            }
            Type genericListType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            return (IDictionary?)Activator.CreateInstance(genericListType);
        }

        /// <summary>
        /// Create a list to hold the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>A List of the given type or null.</returns>
        public static IList? GetListType(Type? type)
        {
            if (type is null)
            {
                return null;
            }
            Type genericListType = typeof(List<>).MakeGenericType(type);
            return (IList?)Activator.CreateInstance(genericListType);
        }

        /// <summary>
        /// Checks if this is a type that OAT Blazor has a component for
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBasicType(Type? type)
        {
            if (type is null)
            {
                return false;
            }
            else if (type == typeof(string) || type == typeof(int) || type == typeof(char) || type == typeof(long) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(bool) ||
                type == typeof(uint) || type == typeof(ulong) || type == typeof(short) || type == typeof(ushort) ||
                type == typeof(DateTime) || type.IsEnum || type.IsArray ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
#if NET5_0_OR_GREATER
                || (type.IsGenericType && type.IsAssignableTo(typeof(System.Runtime.CompilerServices.ITuple)))
#endif
                )
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
        public static bool IsNullable(Type? type) => !(type is null) && Nullable.GetUnderlyingType(type) != null;

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
        /// Recursively checks if the object Type given can be constructed given what is loaded into the AppDomain.
        /// </summary>
        /// <param name="type">The Type to Check</param>
        /// <returns></returns>
        public static bool ConstructedOfLoadedTypes(Type type)
        {
            return type?.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any(x => ConstructedOfLoadedTypes(x)) ?? false;
        }

        /// <summary>
        /// Determines if the ConstructorInfo given is constructable given what is loaded into the AppDomain.
        /// </summary>
        /// <param name="constructorInfo"></param>
        /// <returns>true if only basic types and types derived from basic types can be used to construct.</returns>
        public static bool ConstructedOfLoadedTypes(ConstructorInfo constructorInfo)
        {
            var parameters = constructorInfo.GetParameters();
            if (parameters.Length > 0)
            {
                var loadedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
                return constructorInfo?.GetParameters().Any(x => loadedTypes.Contains(x.ParameterType)) ?? false;
            }
            else
            {
                return true;
            }
        }

        internal static object? GetValueByPropertyOrFieldNameInternal(object? obj, string? propertyName)
        {
            if (propertyName is null) { return null; }
            if (obj is IDictionary dict)
            {
                return dict[propertyName];
            }
            else if (obj is IList list)
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
            var splits = propertyName?.Split('.') ?? Array.Empty<string>();
            foreach (var split in splits)
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
                return Enum.ToObject(type, GetDefaultValueForType(type.GetEnumUnderlyingType())!);
            }
            else if (type == typeof(List<string>))
            {
                return new List<string>();
            }
            else if (type == typeof(List<KeyValuePair<string, string>>))
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
                try
                {
                    return System.Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    return null;
                }
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
            // We keep track of each object along the path to account for ValueTypes
            var objs = new List<(object? obj, string path)>();
            var obj2 = obj;

            var splits = propertyName?.Split('.');
            if (splits != null)
            {
                for (var i = 0; i < splits.Length - 1; i++)
                {
                    objs.Add((obj2, splits[i]));
                    obj2 = GetValueByPropertyOrFieldNameInternal(obj2, splits[i]);
                }
                SetValueByPropertyOrFieldNameInternal(obj2, splits[^1], value);
                objs.Add((obj2, string.Empty));
                for(int i = objs.Count - 2; i >= 0; i--)
                {
                    // We need to explicitly put value types back or we will have only changed a copy
                    if (objs[i + 1].obj is ValueType)
                    {
                        SetValueByPropertyOrFieldNameInternal(objs[i].obj, objs[i].path, objs[i + 1].obj);
                    }
                }
            }
        }

        internal static void SetValueByPropertyOrFieldNameInternal(object? obj, string propertyName, object? value)
        {
            // For scaffolds
            if (obj is IDictionary dict)
            {
                if (dict.Keys.OfType<string>().Any())
                {
                    dict[propertyName] = value!;
                }
            }
            else if (obj is IList list && int.TryParse(propertyName, out var propertyIndex) && list.Count > propertyIndex)
            {
                list[propertyIndex] = value!;
            }
            else
            {
                if (obj?.GetType().GetProperty(propertyName) is PropertyInfo prop)
                {
                    if (prop.CanWrite)
                    {
                        prop.SetValue(obj, value);
                    }
                }
                if (obj?.GetType().GetField(propertyName) is FieldInfo field)
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
                types.AddRange(e.Types.Where(x => x is Type)!);
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
