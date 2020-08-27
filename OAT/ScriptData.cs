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
        public ScriptData(string? code = null, IEnumerable<string>? imports = null, IEnumerable<string>? references = null)
        {
            Code = code ?? string.Empty;
            Imports = imports?.ToList() ?? new List<string>();
            References = references?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// The Script code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The Script Imports
        /// </summary>
        public List<string> Imports { get; set; }
        /// <summary>
        /// The Script References
        /// </summary>
        public List<string> References { get; set; }
        internal string ImportString
        {
            get
            {
                return string.Join(",", Imports);
            }
        }
        internal string ReferencesString
        {
            get
            {
                return string.Join(",", References);
            }
        }

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
        /// <summary>
        /// Get the HashCode for this ScriptData
        /// </summary>
        /// <returns>The HashCode over the Code, Imports and References. Imports and References are order sensitive.</returns>
        public override int GetHashCode()
        {
            return (Code, ImportString, ReferencesString).GetHashCode();
        }
    }
}
