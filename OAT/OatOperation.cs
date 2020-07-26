using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CST.OAT
{
    /// <summary>
    /// This delegate allows extending the Analyzer with a custom operation.
    /// </summary>
    /// <param name="clause">The clause being applied</param>
    /// <param name="state1">The first object state</param>
    /// <param name="state2">The second object state</param>
    /// <param name="captures">The previously found clause captures</param>
    /// <returns>(If the Operation delegate applies to the clause, If the operation was successful, if capturing is enabled the ClauseCapture)</returns>
    public delegate OperationResult OperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures);

    /// <summary>
    /// This delegate allows extending the Analyzer with extra rule validation for custom rules.
    /// </summary>
    /// <param name="rule">The Target Rule</param>
    /// <param name="clause">The Target Clause</param>
    /// <returns>(If the validation applied, The Enumerable of Violations found)</returns>
    public delegate IEnumerable<Violation> ValidationDelegate(Rule rule, Clause clause);
    public class OatOperation
    {
        public OatOperation(Operation operation, OperationDelegate operationDelegate, ValidationDelegate validationDelegate, Analyzer analyzer, string? customOperation = null) : this(operation,analyzer)
        {
            OperationDelegate = operationDelegate;
            ValidationDelegate = validationDelegate;
            CustomOperation = customOperation;
        }

        public OatOperation(Operation operation, Analyzer analyzer)
        {
            Operation = operation;
            Analyzer = analyzer;
        }

        public Analyzer Analyzer { get; set; }
        public ValidationDelegate ValidationDelegate { get; set; }
        public Operation Operation { get; set; } = Operation.Custom;
        public OperationDelegate OperationDelegate { get; set; } = NopOperation;
        public string? CustomOperation { get; set; }

        internal string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_key))
                {
                    _key = string.Format("{0}{1}{2}", Operation, CustomOperation is null ? "" : " - ", CustomOperation is null ? "" : CustomOperation);
                }
                return _key;
            }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="state1"></param>
        /// <param name="state2"></param>
        /// <returns></returns>
        public static OperationResult NopOperation(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            Log.Debug($"{clause.Operation} is not supported.");
            return new OperationResult(false, null);
        }
        private string _key = "";
    }
}
