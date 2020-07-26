﻿using KellermanSoftware.CompareNetObjects;
using Microsoft.CST.OAT.Utils;
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
    public class WasModifiedOperation : OatOperation
    {
        private readonly ConcurrentDictionary<string, Regex?> RegexCache = new ConcurrentDictionary<string, Regex?>();
        public WasModifiedOperation(Analyzer analyzer) : base(Operation.WasModified, analyzer)
        {
            OperationDelegate = WasModifiedOperationDelegate;
            ValidationDelegate = WasModifiedValidationDelegate;
        }

        private IEnumerable<Violation> WasModifiedValidationDelegate(Rule rule, Clause clause)
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
        internal OperationResult WasModifiedOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            var compareLogic = new CompareLogic();
            // Gather all differences if we are capturing
            compareLogic.Config.MaxDifferences = clause.Capture ? int.MaxValue : 1;

            var comparisonResult = compareLogic.Compare(state1, state2);
            if ((!comparisonResult.AreEqual && !clause.Invert) || (comparisonResult.AreEqual && clause.Invert))
            {
                return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<ComparisonResult>(clause, comparisonResult, state1, state2));
            }
            return new OperationResult(false, null);
        }
    }
}
