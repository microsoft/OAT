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
    public class GreaterThanOperation : OatOperation
    {
        private readonly ConcurrentDictionary<string, Regex?> RegexCache = new ConcurrentDictionary<string, Regex?>();
        public GreaterThanOperation(Analyzer analyzer) : base(Operation.GreaterThan, analyzer)
        {
            OperationDelegate = GreaterThanOperationDelegate;
            ValidationDelegate = GreaterThanValidationDelegate;
        }

        private IEnumerable<Violation> GreaterThanValidationDelegate(Rule rule, Clause clause)
        {
            if (clause.Data?.Count == null || clause.Data is List<string> clauseList && (clauseList.Count != 1 || !int.TryParse(clause.Data.First(), out int _)))
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseExpectedInt"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
            }
            if (clause.DictData != null || clause.DictData?.Count > 0)
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
            }
        }
        internal OperationResult GreaterThanOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            (var stateOneList, _) = Analyzer.ObjectToValues(state1);
            (var stateTwoList, _) = Analyzer.ObjectToValues(state2);

            foreach (var val in stateOneList)
            {
                if (int.TryParse(val, out int valToCheck)
                        && int.TryParse(clause.Data?[0], out int dataValue)
                        && ((valToCheck > dataValue) || (clause.Invert && valToCheck <= dataValue)))
                {
                    return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<int>(clause, valToCheck, state1, null));
                }
            }
            foreach (var val in stateTwoList)
            {
                if (int.TryParse(val, out int valToCheck)
                    && int.TryParse(clause.Data?[0], out int dataValue)
                    && ((valToCheck > dataValue) || (clause.Invert && valToCheck <= dataValue)))
                {
                    return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<int>(clause, valToCheck, null, state2));
                }
            }
            return new OperationResult(false, null);
        }
    }
}
