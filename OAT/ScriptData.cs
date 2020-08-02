using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CST.OAT
{
    public class ScriptData
    {
        public ScriptData(string code, IEnumerable<string> imports, IEnumerable<string> references)
        {
            Code = code;
            Imports = imports.ToArray();
            References = references.ToArray();
            ImportString = string.Join(",", Imports);
            ReferencesString = string.Join(",", References);
            _hashCode = (Code, ImportString, ReferencesString).GetHashCode();
        }

        public string Code { get; }
        public string[] Imports { get; }
        public string[] References { get; }
        public string ImportString { get; }
        public string ReferencesString { get; }

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

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
