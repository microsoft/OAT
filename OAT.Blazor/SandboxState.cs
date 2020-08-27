using System.Collections.Generic;

namespace Microsoft.CST.OAT.Blazor
{
    public class SandboxState
    {
        /// <summary>
        /// A Dictionary of FullName of object (with namespace)
        /// to list of that objects of that Type
        /// </summary>
        public Dictionary<string, List<object>> Objects { get; set; } = new Dictionary<string, List<object>>();

        /// <summary>
        /// The names of assemblies that must be loaded to use this SandboxState
        /// </summary>
        public List<string> AssemblyNames { get; set; } = new List<string>();

        public SandboxState()
        {
        }

        public SandboxState(List<string> assemblyNames, Dictionary<string, List<object>> objects)
        {
            AssemblyNames = assemblyNames;
            Objects = objects;
        }
    }
}
