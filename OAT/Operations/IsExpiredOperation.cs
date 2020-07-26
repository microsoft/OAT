﻿using Microsoft.CST.OAT.Utils;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CST.OAT
{
    public class IsExpiredOperation : OatOperation
    {
        private readonly ConcurrentDictionary<string, Regex?> RegexCache = new ConcurrentDictionary<string, Regex?>();
        public IsExpiredOperation(Analyzer analyzer) : base(Operation.IsExpired, analyzer)
        {
            OperationDelegate = IsExpiredOperationDelegate;
            ValidationDelegate = IsExpiredValidationDelegate;
        }

        private IEnumerable<Violation> IsExpiredValidationDelegate(Rule rule, Clause clause)
        {
            if (!(clause.Data?.Count == null || clause.Data?.Count == 0))
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseRedundantData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
            }
            else if (!(clause.DictData?.Count == null || clause.DictData?.Count == 0))
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseRedundantDictData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
            }
        }
        internal OperationResult IsExpiredOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            if (state1 is DateTime date1)
            {
                var res = date1.CompareTo(DateTime.Now) < 0;
                if ((res && !clause.Invert) || (clause.Invert && !res))
                    return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<DateTime>(clause, date1, state1, null));
            }
            if (state2 is DateTime date2)
            {
                var res = date2.CompareTo(DateTime.Now) < 0;
                if ((res && !clause.Invert) || (clause.Invert && !res))
                    return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<DateTime>(clause, date2, null, state2));
            }
            return new OperationResult(false, null);
        }
    }
}
