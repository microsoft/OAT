using Microsoft.CST.OAT.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;

namespace Microsoft.CST.OAT.Blazor.Components
{
    public class Scaffold
    {
        public Dictionary<string, object?> Parameters { get; } = new Dictionary<string, object?>();
        public ConstructorInfo Constructor { get; }

        public Scaffold(ConstructorInfo constructorToUse)
        {
            Constructor = constructorToUse;

            foreach (var parameter in Constructor.GetParameters() ?? Array.Empty<ParameterInfo>())
            {
                if (parameter.HasDefaultValue)
                {
                    Parameters.Add(parameter.Name, parameter.DefaultValue);
                }
                else
                {
                    if (Helpers.IsBasicType(parameter.ParameterType))
                    {
                        Parameters.Add(parameter.Name, Helpers.GetDefaultValueForType(parameter.ParameterType));
                    }
                    else
                    {
                        if (parameter.ParameterType.GetConstructors().Where(x => Helpers.ConstructedOfBasicTypes(x)).FirstOrDefault() is ConstructorInfo constructor)
                        {
                            Parameters.Add(parameter.Name, new Scaffold(constructor));
                        }
                        else
                        {
                            Parameters.Add(parameter.Name, null);
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
                if (Parameters[parameter.Name] is Scaffold scaffoldedState)
                {
                    inputs.Add(scaffoldedState.Construct());
                }
                else
                {
                    inputs.Add(Parameters[parameter.Name]);
                }
            }
            return Constructor?.Invoke(inputs.ToArray());
        }
    }
}
