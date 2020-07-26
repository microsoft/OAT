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
    public class EqualsOperation : OatOperation
    {
        private readonly ConcurrentDictionary<string, Regex?> RegexCache = new ConcurrentDictionary<string, Regex?>();
        public EqualsOperation(Analyzer analyzer) : base(Operation.Equals, analyzer)
        {
            OperationDelegate = EqualsOperationDelegate;
            ValidationDelegate = EqualsValidationDelegate;
        }

        private IEnumerable<Violation> EqualsValidationDelegate(Rule rule, Clause clause)
        {
            if ((clause.Data?.Count == null || clause.Data?.Count == 0))
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseNoData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
            }
            if (clause.DictData != null || clause.DictData?.Count > 0)
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
            }
        }
        internal OperationResult EqualsOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            (var stateOneList, _) = Analyzer.ObjectToValues(state1);
            (var stateTwoList, _) = Analyzer.ObjectToValues(state2);
            if (clause.Data is List<string> EqualsData)
            {
                List<string> StateListToEqList(List<string> stateList)
                {
                    var results = new List<string>();
                    foreach (var datum in EqualsData)
                    {
                        foreach (var stateOneDatum in stateList)
                        {
                            if (clause.Invert && stateOneDatum != datum)
                            {
                                results.Add(stateOneDatum);
                            }
                            else if (!clause.Invert && stateOneDatum == datum)
                            {
                                results.Add(stateOneDatum);
                            }
                        }
                    }
                    return results;
                }

                var res = StateListToEqList(stateOneList);
                if (res.Any())
                {
                    var typeHolder = state1 ?? state2;

                    return typeHolder switch
                    {
                        string _ => new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<string>(clause, res.First(), state1, null)),
                        _ => new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<List<string>>(clause, res, state1, null)),
                    };
                }
                res = StateListToEqList(stateTwoList);
                if (res.Any())
                {
                    var typeHolder = state1 ?? state2;

                    return typeHolder switch
                    {
                        string _ => new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<string>(clause, res.First(), null, state2)),
                        _ => new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<List<string>>(clause, res, null, state2)),
                    };
                }
            }
            return new OperationResult(false, null);
        }
    }
}
