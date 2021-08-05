using Microsoft.CST.OAT.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.CST.OAT.Blazor.Components
{
    public class Scaffold
    {
        public List<(string name, object? obj, Type type)> Parameters { get; } = new List<(string, object?, Type)>();
        public ConstructorInfo Constructor { get; }

        public Scaffold(ConstructorInfo constructorToUse)
        {
            Constructor = constructorToUse;

            foreach (var parameter in Constructor.GetParameters() ?? Array.Empty<ParameterInfo>())
            {
                var name = parameter.Name ?? string.Empty;
                if (parameter.HasDefaultValue)
                {
                    Parameters.Add((name, parameter.DefaultValue, parameter.ParameterType));
                }
                else
                {
                    if (Helpers.IsBasicType(parameter.ParameterType))
                    {
                        Parameters.Add((name, Helpers.GetDefaultValueForType(parameter.ParameterType), parameter.ParameterType));
                    }
                    else
                    {
                        if (parameter.ParameterType.GetConstructors().Where(x => Helpers.ConstructedOfLoadedTypes(x)).FirstOrDefault() is ConstructorInfo constructor)
                        {
                            Parameters.Add((name, new Scaffold(constructor), parameter.ParameterType));
                        }
                        else
                        {
                            Parameters.Add((name, null, parameter.ParameterType));
                        }
                    }
                }
            }
        }

        public object? Construct()
        {
            var inputs = Parameters.Select(x => x.obj is Scaffold s ? s.Construct() : x.obj);
            return Constructor?.Invoke(inputs.ToArray());
        }
    }
}
