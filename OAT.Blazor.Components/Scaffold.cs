using Microsoft.CST.OAT.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.CST.OAT.Blazor.Components
{
    public class Scaffold
    {
        public Dictionary<string, (object? obj, Type type)> Parameters { get; } = new Dictionary<string, (object?, Type)>();
        public ConstructorInfo Constructor { get; }

        public Scaffold(ConstructorInfo constructorToUse, IEnumerable<Assembly>? assemblies = null)
        {
            Constructor = constructorToUse;

            foreach (var parameter in Constructor.GetParameters() ?? Array.Empty<ParameterInfo>())
            {
                if (parameter.Name is null)
                {
                    continue;
                }
                if (parameter.HasDefaultValue)
                {
                    Parameters.Add(parameter.Name, (parameter.DefaultValue, parameter.ParameterType));
                }
                else
                {
                    if (Helpers.IsBasicType(parameter.ParameterType))
                    {
                        Parameters.Add(parameter.Name, (Helpers.GetDefaultValueForType(parameter.ParameterType), parameter.ParameterType));
                    }
                    else
                    {
                        if (parameter.ParameterType.GetConstructors().Where(x => Helpers.ConstructedOfLoadedTypes(x, assemblies)).FirstOrDefault() is ConstructorInfo constructor)
                        {
                            Parameters.Add(parameter.Name, (new Scaffold(constructor, assemblies), parameter.ParameterType));
                        }
                        else
                        {
                            Parameters.Add(parameter.Name, (null, parameter.ParameterType));
                        }
                    }
                }
            }
        }

        public object? Construct()
        {
            var inputs = new List<object?>();
            foreach (var parameter in Constructor?.GetParameters() ?? Array.Empty<ParameterInfo>())
            {
                if (parameter.Name is null) 
                { 
                    continue; 
                }
                if (Parameters[parameter.Name].obj is Scaffold scaffoldedState)
                {
                    inputs.Add(scaffoldedState.Construct());
                }
                else
                {
                    inputs.Add(Parameters[parameter.Name].obj);
                }
            }
            return Constructor?.Invoke(inputs.ToArray());
        }
    }
}
