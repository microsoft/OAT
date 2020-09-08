using Microsoft.CST.OAT.Utils;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.CST.OAT.Operations
{
    /// <summary>
    /// An operation that yields a validation error indicating scripting is disabled.
    /// </summary>
    public class ScriptsDisabledOperation : OatOperation
    {
        /// <summary>
        /// Create a ScriptsDisabledOperation
        /// </summary>
        /// <param name="analyzer"></param>
        public ScriptsDisabledOperation(Analyzer analyzer) : base(Operation.Script, analyzer)
        {
            OperationDelegate = ScriptOperationDelegate;
            ValidationDelegate = ScriptOperationValidationDelegate;
        }

        internal IEnumerable<Violation> ScriptOperationValidationDelegate(Rule rule, Clause clause)
        {
            yield return new Violation(string.Format(Strings.Get("Err_ScriptingDisabled_{0}{1}"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
        }

        internal OperationResult ScriptOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            return new OperationResult(false, null);
        }
    }
}
