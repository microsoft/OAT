using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CST.OAT
{
    /// <summary>
    /// A holder for a Script to run for a clause
    /// </summary>
    public class ScriptData
    {
        /// <summary>
        /// The constructor for a Script to run for a clause.
        /// </summary>
        /// <param name="code">The Code to run</param>
        /// <param name="imports">The imports to include. For example "System.IO".</param>
        /// <param name="references">The assembly references to include.  For example "MyAssembly" for "MyAssembly.dll"</param>
        public ScriptData(string code, IEnumerable<string>? imports = null, IEnumerable<string>? references = null)
        {
            Code = code ?? string.Empty;
            Imports = imports?.ToArray() ?? Array.Empty<string>();
            References = references?.ToArray() ?? Array.Empty<string>();
            ImportString = string.Join(",", Imports);
            ReferencesString = string.Join(",", References);
            _hashCode = (Code, ImportString, ReferencesString).GetHashCode();
        }

        /// <summary>
        /// The Script code
        /// </summary>
        public string Code { get; }
        /// <summary>
        /// The Script Imports
        /// </summary>
        public string[] Imports { get; }
        /// <summary>
        /// The Script References
        /// </summary>
        public string[] References { get; }
        internal string ImportString { get; }
        internal string ReferencesString { get; }

        /// <summary>
        /// Overridden Equality constructor
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if obj is a ScriptData and the Code, Imports and References match.  Imports and References are order sensitive.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ScriptData scriptIn)
            {
                if (Code == scriptIn.Code)
                {
                    if (ImportString == scriptIn.ImportString)
                    {
                        if (ReferencesString == scriptIn.ReferencesString)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private int _hashCode;

        /// <summary>
        /// Get the HashCode for this ScriptData
        /// </summary>
        /// <returns>The HashCode over the Code, Imports and References. Imports and References are order sensitive.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
