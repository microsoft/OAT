using Microsoft.CST.OAT.Utils;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CST.OAT.Operations
{
    /// <summary>
    /// The default Regex operation
    /// </summary>
    public class RegexOperation : OatOperation
    {
        private readonly ConcurrentDictionary<string, Regex?> RegexCache = new ConcurrentDictionary<string, Regex?>();

        /// <summary>
        /// Create an OatOperation given an analyzer
        /// </summary>
        /// <param name="analyzer">The analyzer context to work with</param>
        public RegexOperation(Analyzer analyzer) : base(Operation.Regex, analyzer)
        {
            OperationDelegate = RegexOperationDelegate;
            ValidationDelegate = RegexValidationDelegate;
        }

        private IEnumerable<Violation> RegexValidationDelegate(Rule rule, Clause clause)
        {
            if (clause.Data?.Count == null || clause.Data?.Count == 0)
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseNoData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
            }
            else if (clause.Data is List<string> regexList)
            {
                foreach (string? regex in regexList)
                {
                    if (!Helpers.IsValidRegex(regex))
                    {
                        yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidRegex"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), regex), rule, clause);
                    }
                }
            }
            if (clause.DictData != null || clause.DictData?.Count > 0)
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
            }
        }

        internal OperationResult RegexOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            (List<string>? stateOneList, _) = Analyzer?.ObjectToValues(state1) ?? (new List<string>(), new List<KeyValuePair<string, string>>());
            (List<string>? stateTwoList, _) = Analyzer?.ObjectToValues(state2) ?? (new List<string>(), new List<KeyValuePair<string, string>>());
            if (clause.Data is List<string> RegexList && RegexList.Any())
            {
                string? built = string.Join("|", RegexList);

                Regex? regex = StringToRegex(built);

                if (regex != null)
                {
                    foreach (string? state in stateOneList)
                    {
                        MatchCollection? matches = regex.Matches(state);

                        if (matches.Count > 0 || (matches.Count == 0 && clause.Invert))
                        {
                            List<Match>? outmatches = new List<Match>();
                            foreach (object? match in matches)
                            {
                                if (match is Match m)
                                {
                                    outmatches.Add(m);
                                }
                            }
                            return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<List<Match>>(clause, outmatches, state1));
                        }
                    }
                    foreach (string? state in stateTwoList)
                    {
                        MatchCollection? matches = regex.Matches(state);

                        if (matches.Count > 0 || (matches.Count == 0 && clause.Invert))
                        {
                            List<Match>? outmatches = new List<Match>();
                            foreach (object? match in matches)
                            {
                                if (match is Match m)
                                {
                                    outmatches.Add(m);
                                }
                            }
                            return new OperationResult(true, !clause.Capture ? null : new TypedClauseCapture<List<Match>>(clause, outmatches, state2: state2));
                        }
                    }
                }
            }
            return new OperationResult(false, null);
        }
        /// <summary>
        /// Converts a strings to a compiled regex.
        /// Uses an internal cache.
        /// </summary>
        /// <param name="built">The regex to build</param>
        /// <returns>The built Regex</returns>
        public Regex? StringToRegex(string built)
        {
            if (!RegexCache.ContainsKey(built))
            {
                try
                {
                    RegexCache.TryAdd(built, new Regex(built, RegexOptions.Compiled));
                }
                catch (ArgumentException)
                {
                    Log.Warning("InvalidArgumentException when creating regex. Regex {0} is invalid and will be skipped.", built);
                    RegexCache.TryAdd(built, null);
                }
            }
            return RegexCache[built];
        }
    }
}
