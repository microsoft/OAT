using Microsoft.CST.OAT.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.CST.OAT.Operations
{
    /// <summary>
    ///     The default NoOperation operation
    /// </summary>
    public class NoOperation : OatOperation
    {
        /// <summary>
        ///     Create an OatOperation given an analyzer
        /// </summary>
        /// <param name="analyzer"> The analyzer context to work with </param>
        public NoOperation(Analyzer analyzer) : base(Operation.NoOperation, analyzer)
        {
            OperationDelegate = NoOperationOperationDelegate;
            ValidationDelegate = NoOperationValidationDelegate;
        }

        internal OperationResult NoOperationOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            Log.Warning("Clause {0} has Operation NoOperation.", clause.Label);
            return new OperationResult(false);
        }

        internal IEnumerable<Violation> NoOperationValidationDelegate(Rule rule, Clause clause)
        {
            yield return new Violation(string.Format(Strings.Get("Err_NoOperation_{0}{1}"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString()), rule, clause);
        }
    }
}