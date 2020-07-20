// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using KellermanSoftware.CompareNetObjects;
using Microsoft.CST.OAT.Utils;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.CST.OAT
{
    /// <summary>
    /// This is the core engine of OAT
    /// </summary>
    public class Analyzer
    {
        private readonly ConcurrentDictionary<string, Regex?> RegexCache = new ConcurrentDictionary<string, Regex?>();

        /// <summary>
        /// The constructor for Analyzer takes no arguments.
        /// </summary>
        public Analyzer()
        {
        }

        /// <summary>
        /// This delegate is for iterating into complex objects like dictionaries that the Analyzer doesn't natively understand
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="index">String based index into the object</param>
        /// <returns>(If we successfully extracted, The extraction result)</returns>
        public delegate (bool Processed, object? Result) PropertyExtractionDelegate(object? obj, string index);

        /// <summary>
        /// This delegate is for turning complex objects like dictionaries that the Analyzer doesn't natively support into a dictionary or list of strings that OAT can use for default operations
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <returns>(If the object was parsed, A list of Strings that were extracted, A List of KVP that were extracted)</returns>
        public delegate (bool Processed, IEnumerable<string> valsExtracted, IEnumerable<KeyValuePair<string, string>> dictExtracted) ObjectToValuesDelegate(object? obj);

        /// <summary>
        /// This delegate allows extending the Analyzer with a custom operation.
        /// </summary>
        /// <param name="clause">The clause being applied</param>
        /// <param name="valsToCheck">The list of strings that have been extracted</param>
        /// <param name="dictToCheck">The list of KVP of strings that have been extracted</param>
        /// <param name="state1">The first object state</param>
        /// <param name="state2">The second object state</param>
        /// <returns>(If the Operation delegate applies to the clause, If the operation was successful)</returns>
        public delegate (bool Applies, bool Result) OperationDelegate(Clause clause, IEnumerable<string>? valsToCheck, IEnumerable<KeyValuePair<string, string>> dictToCheck, object? state1, object? state2);

        /// <summary>
        /// This delegate allows extending the Analyzer with extra rule validation for custom rules.
        /// </summary>
        /// <param name="r">The Target Rule</param>
        /// <param name="c">The Target Clause</param>
        /// <returns>(If the validation applied, The Enumerable of Violations found)</returns>
        public delegate (bool Applies, IEnumerable<Violation> FoundViolations) ValidationDelegate(Rule r, Clause c);

        /// <summary>
        /// The PropertyExtractionDelegates that will be used in order of attempt.  Once successful the others won't be run.
        /// </summary>
        public List<PropertyExtractionDelegate> CustomPropertyExtractionDelegates { get; set; } = new List<PropertyExtractionDelegate>();

        /// <summary>
        /// The ObjectToValuesDelegates that will be used in order of attempt. Once successful the others won't be run.
        /// </summary>
        public List<ObjectToValuesDelegate> CustomObjectToValuesDelegates { get; set; } = new List<ObjectToValuesDelegate>();

        /// <summary>
        /// The OperationDelegates that will be used in order of attempt.  Once successful the others won't be run.
        /// </summary>
        public List<OperationDelegate> CustomOperationDelegates { get; set; } = new List<OperationDelegate>();

        /// <summary>
        /// The ValidationDelegates that will be used in order of attempt.  All will be run. Order not guaranteed.
        /// </summary>
        public List<ValidationDelegate> CustomOperationValidationDelegates { get; set; } = new List<ValidationDelegate>();

        /// <summary>
        /// Extracts a value stored at the specified path inside an object. Can crawl into Lists and
        /// Dictionaries of strings and return any top-level object.
        /// </summary>
        /// <param name="targetObject">The object to parse</param>
        /// <param name="pathToProperty">The path of the property to fetch</param>
        /// <returns>The object found</returns>
        public object? GetValueByPropertyString(object? targetObject, string pathToProperty)
        {
            if (pathToProperty is null || targetObject is null)
            {
                return null;
            }
            try
            {
                var pathPortions = pathToProperty.Split('.');

                // We first try to get the first value to get it started
                var value = GetValueByPropertyOrFieldName(targetObject, pathPortions[0]);

                // For the rest of the path we walk each portion to get the next object
                for (int pathPortionIndex = 1; pathPortionIndex < pathPortions.Length; pathPortionIndex++)
                {
                    if (value == null) { break; }

                    switch (value)
                    {
                        case Dictionary<string, string> stringDict:
                            if (stringDict.TryGetValue(pathPortions[pathPortionIndex], out string? stringValue))
                            {
                                value = stringValue;
                            }
                            else
                            {
                                value = null;
                            }
                            break;

                        case List<string> stringList:
                            if (int.TryParse(pathPortions[pathPortionIndex], out int ArrayIndex) && stringList.Count > ArrayIndex)
                            {
                                value = stringList[ArrayIndex];
                            }
                            else
                            {
                                value = null;
                            }
                            break;

                        default:
                            (bool Processed, object? Result)? res = null;
                            var found = false;
                            foreach (var del in CustomPropertyExtractionDelegates)
                            {
                                res = del?.Invoke(value, pathPortions[pathPortionIndex]);
                                if (res.HasValue && res.Value.Processed)
                                {
                                    found = true;
                                    value = res.Value.Result;
                                    break;
                                }
                            }

                            // If we couldn't do any custom parsing fall back to the default
                            if (!found)
                            {
                                value = GetValueByPropertyOrFieldName(value, pathPortions[pathPortionIndex]);
                            }
                            break;
                    }
                }
                return value;
            }
            catch (Exception e)
            {
                Log.Information("Fetching Field {0} failed from {1} ({2}:{3})", pathToProperty, targetObject.GetType(), e.GetType(), e.Message);
            }
            return null;
        }

        /// <summary>
        ///     Prints out the Enumerable of violations to Warning
        /// </summary>
        /// <param name="violations">An Enumerable of Violations to print</param>
        public static void PrintViolations(IEnumerable<Violation> violations)
        {
            if (violations == null) return;
            foreach (var violation in violations)
            {
                Log.Warning(violation.description);
            }
        }

        /// <summary>
        ///     Get the Tags which apply to the object given the Rules
        /// </summary>
        /// <param name="rules">The Rules to apply</param>
        /// <param name="state1">The first state of the object</param>
        /// <param name="state2">The second state of the object</param>
        /// <returns></returns>
        public string[] GetTags(IEnumerable<Rule> rules, object? state1 = null, object? state2 = null)
        {
            var tags = new ConcurrentDictionary<string, byte>();

            Parallel.ForEach(rules, rule =>
            {
                // If there are no tags, or all of the tags are already in the tags we've found skip otherwise apply.
                if ((!rule.Tags.Any() || !rule.Tags.All(x => tags.Keys.Any(y => y == x))) && Applies(rule, state1, state2))
                {
                    foreach (var tag in rule.Tags)
                    {
                        tags.TryAdd(tag, 0);
                    }
                }
            });

            return tags.Keys.ToArray();
        }

        /// <summary>
        ///     Which rules apply to this object given up to two states?
        /// </summary>
        /// <param name="rules">The rules to apply</param>
        /// <param name="state1">The first state</param>
        /// <param name="state2">The second state</param>
        /// <returns>A Stack of Rules which apply</returns>
        public ConcurrentStack<Rule> Analyze(IEnumerable<Rule> rules, object? state1 = null, object? state2 = null)
        {
            var results = new ConcurrentStack<Rule>();

            Parallel.ForEach(rules, rule =>
            {
                if (Applies(rule, state1, state2))
                {
                    results.Push(rule);
                }
            });

            return results;
        }

        /// <summary>
        ///     Does the rule apply to the object?
        /// </summary>
        /// <param name="rule">The Rule to apply</param>
        /// <param name="state1">The first state of the object</param>
        /// <param name="state2">The second state of the object</param>
        /// <returns>True if the rule applies</returns>
        public bool Applies(Rule rule, object? state1 = null, object? state2 = null)
        {
            if (rule != null)
            {
                var sample = state1 is null ? state2 : state1;

                // Does the name of this class match the Target in the rule?
                // Or has no target been specified (match all)
                if (rule.Target is null || (sample?.GetType().Name.Equals(rule.Target, StringComparison.InvariantCultureIgnoreCase) ?? true))
                {
                    // If the expression is null the default is that all clauses must be true
                    // If we have no clauses .All will still match
                    if (rule.Expression is null)
                    {
                        if (rule.Clauses.All(x => AnalyzeClause(x, state1, state2)))
                        {
                            return true;
                        }
                    }
                    // Otherwise we evaluate the expression
                    else
                    {
                        if (Evaluate(rule.Expression.Split(' '), rule.Clauses, state1, state2))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        /// <summary>
        /// Determines if there are any problems with the provided rule.
        /// </summary>
        /// <param name="rule">The rule to parse.</param>
        /// <returns>True if there are no issues.</returns>
        public bool IsRuleValid(Rule rule) => !EnumerateRuleIssues(new Rule[] { rule }).Any();

        /// <summary>
        /// Verifies the provided rules and provides a list of issues with the rules.
        /// </summary>
        /// <param name="rules">Enumerable of Rules.</param>
        /// <returns>Enumerable of issues with the rules.</returns>
        public IEnumerable<Violation> EnumerateRuleIssues(IEnumerable<Rule> rules)
        {
            if (!Strings.IsLoaded)
            {
                Strings.Setup();
            }
            foreach (Rule rule in rules ?? Array.Empty<Rule>())
            {
                var clauseLabels = rule.Clauses.GroupBy(x => x.Label);

                // If clauses have duplicate names
                var duplicateClauses = clauseLabels.Where(x => x.Key != null && x.Count() > 1);
                foreach (var duplicateClause in duplicateClauses)
                {
                    yield return new Violation(string.Format(Strings.Get("Err_ClauseDuplicateName"), rule.Name, duplicateClause.Key ?? string.Empty), rule, duplicateClause.AsEnumerable().ToArray());
                }

                // If clause label contains illegal characters
                foreach (var clause in rule.Clauses)
                {
                    if (clause.Label is string label)
                    {
                        if (label.Contains(" ") || label.Contains("(") || label.Contains(")"))
                        {
                            yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidLabel"), rule.Name, label), rule, clause);
                        }
                    }
                    switch (clause.Operation)
                    {
                        case OPERATION.EQ:
                        case OPERATION.NEQ:
                            if ((clause.Data?.Count == null || clause.Data?.Count == 0))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseNoData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            if (clause.DictData != null || clause.DictData?.Count > 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
                            }
                            break;

                        case OPERATION.CONTAINS:
                        case OPERATION.CONTAINS_ANY:
                            if ((clause.Data?.Count == null || clause.Data?.Count == 0) && (clause.DictData?.Count == null || clause.DictData?.Count == 0))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseNoDataOrDictData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            if ((clause.Data is List<string> list && list.Count > 0) && (clause.DictData is List<KeyValuePair<string, string>> dictList && dictList.Count > 0))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseBothDataDictData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            break;

                        case OPERATION.ENDS_WITH:
                        case OPERATION.STARTS_WITH:
                            if (clause.Data?.Count == null || clause.Data?.Count == 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseNoData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            if (clause.DictData != null || clause.DictData?.Count > 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
                            }
                            break;

                        case OPERATION.GT:
                        case OPERATION.LT:
                            if (clause.Data?.Count == null || clause.Data is List<string> clauseList && (clauseList.Count != 1 || !int.TryParse(clause.Data.First(), out int _)))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseExpectedInt"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            if (clause.DictData != null || clause.DictData?.Count > 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
                            }
                            break;

                        case OPERATION.REGEX:
                            if (clause.Data?.Count == null || clause.Data?.Count == 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseNoData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            else if (clause.Data is List<string> regexList)
                            {
                                foreach (var regex in regexList)
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
                            break;

                        case OPERATION.IS_NULL:
                        case OPERATION.IS_TRUE:
                        case OPERATION.IS_EXPIRED:
                        case OPERATION.WAS_MODIFIED:
                            if (!(clause.Data?.Count == null || clause.Data?.Count == 0))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseRedundantData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            else if (!(clause.DictData?.Count == null || clause.DictData?.Count == 0))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseRedundantDictData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            break;

                        case OPERATION.IS_BEFORE:
                        case OPERATION.IS_AFTER:
                            if (clause.Data?.Count == null || clause.Data is List<string> clauseList2 && (clauseList2.Count != 1 || !DateTime.TryParse(clause.Data.First(), out DateTime _)))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseExpectedDateTime"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            if (clause.DictData != null || clause.DictData?.Count > 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseDictDataUnexpected"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
                            }
                            break;

                        case OPERATION.CONTAINS_KEY:
                            if (clause.DictData != null)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseUnexpectedDictData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            if (clause.Data == null || clause.Data?.Count == 0)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseMissingListData"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            break;

                        case OPERATION.CUSTOM:
                            if (clause.CustomOperation == null)
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseMissingCustomOperation"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                            }
                            else
                            {
                                bool covered = false;
                                foreach (var del in CustomOperationValidationDelegates)
                                {
                                    var res = del?.Invoke(rule, clause) ?? (false, new List<Violation>());
                                    if (res.Applies)
                                    {
                                        covered = true;
                                        foreach (var violation in res.FoundViolations)
                                        {
                                            yield return violation;
                                        }
                                    }
                                }
                                if (covered == false)
                                {
                                    yield return new Violation(string.Format(Strings.Get("Err_ClauseMissingValidationForOperation"), clause.CustomOperation, rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                                }
                            }
                            break;

                        case OPERATION.DOES_NOT_CONTAIN:
                        case OPERATION.DOES_NOT_CONTAIN_ALL:
                        default:
                            yield return new Violation(string.Format(Strings.Get("Err_ClauseUnsuppportedOperator"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString()), rule, clause);
                            break;
                    }
                }

                var foundLabels = new List<string>();

                if (rule.Expression is string expression)
                {
                    // Are parenthesis balanced Are spaces correct Are all variables defined by
                    // clauses? Are variables and operators alternating?
                    var splits = expression.Split(' ');
                    int foundStarts = 0;
                    int foundEnds = 0;
                    bool expectingOperator = false;
                    for (int i = 0; i < splits.Length; i++)
                    {
                        foundStarts += splits[i].Count(x => x.Equals('('));
                        foundEnds += splits[i].Count(x => x.Equals(')'));
                        if (foundEnds > foundStarts)
                        {
                            yield return new Violation(string.Format(Strings.Get("Err_ClauseUnbalancedParentheses"), expression, rule.Name), rule);
                        }
                        // Variable
                        if (!expectingOperator)
                        {
                            var lastOpen = -1;
                            var lastClose = -1;

                            for (int j = 0; j < splits[i].Length; j++)
                            {
                                // Check that the parenthesis are balanced
                                if (splits[i][j] == '(')
                                {
                                    // If we've seen a ) this is now invalid
                                    if (lastClose != -1)
                                    {
                                        yield return new Violation(string.Format(Strings.Get("Err_ClauseParenthesisInLabel"), expression, rule.Name, splits[i]), rule);
                                    }
                                    // If there were any characters between open parenthesis
                                    if (j - lastOpen != 1)
                                    {
                                        yield return new Violation(string.Format(Strings.Get("Err_ClauseCharactersBetweenOpenParentheses"), expression, rule.Name, splits[i]), rule);
                                    }
                                    // If there was a random parenthesis not starting the variable
                                    else if (j > 0)
                                    {
                                        yield return new Violation(string.Format(Strings.Get("Err_ClauseCharactersBeforeOpenParentheses"), expression, rule.Name, splits[i]), rule);
                                    }
                                    lastOpen = j;
                                }
                                else if (splits[i][j] == ')')
                                {
                                    // If we've seen a close before update last
                                    if (lastClose != -1 && j - lastClose != 1)
                                    {
                                        yield return new Violation(string.Format(Strings.Get("Err_ClauseCharactersBetweenClosedParentheses"), expression, rule.Name, splits[i]), rule);
                                    }
                                    lastClose = j;
                                }
                                else
                                {
                                    // If we've set a close this is invalid because we can't have
                                    // other characters after it
                                    if (lastClose != -1)
                                    {
                                        yield return new Violation(string.Format(Strings.Get("Err_ClauseCharactersAfterClosedParentheses"), expression, rule.Name, splits[i]), rule);
                                    }
                                }
                            }

                            var variable = splits[i].Replace("(", "").Replace(")", "");

                            if (variable == "NOT")
                            {
                                if (splits[i].Contains(")"))
                                {
                                    yield return new Violation(string.Format(Strings.Get("Err_ClauseCloseParenthesesInNot"), expression, rule.Name, splits[i]), rule);
                                }
                            }
                            else
                            {
                                foundLabels.Add(variable);
                                if (string.IsNullOrWhiteSpace(variable) || !rule.Clauses.Any(x => x.Label == variable))
                                {
                                    yield return new Violation(string.Format(Strings.Get("Err_ClauseUndefinedLabel"), expression, rule.Name, splits[i].Replace("(", "").Replace(")", "")), rule);
                                }
                                expectingOperator = true;
                            }
                        }
                        //Operator
                        else
                        {
                            // If we can't enum parse the operator
                            if (!Enum.TryParse<BOOL_OPERATOR>(splits[i], out BOOL_OPERATOR op))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidOperator"), expression, rule.Name, splits[i]), rule);
                            }
                            // We don't allow NOT operators to modify other Operators, so we can't
                            // allow NOT here
                            else
                            {
                                if (op is BOOL_OPERATOR boolOp && boolOp == BOOL_OPERATOR.NOT)
                                {
                                    yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidNotOperator"), expression, rule.Name), rule);
                                }
                            }
                            expectingOperator = false;
                        }
                    }

                    // We should always end on expecting an operator (having gotten a variable)
                    if (!expectingOperator)
                    {
                        yield return new Violation(string.Format(Strings.Get("Err_ClauseEndsWithOperator"), expression, rule.Name), rule);
                    }
                }

                // Were all the labels declared in clauses used?
                foreach (var label in rule.Clauses.Select(x => x.Label))
                {
                    if (label is string)
                    {
                        if (!foundLabels.Contains(label))
                        {
                            yield return new Violation(string.Format(Strings.Get("Err_ClauseUnusedLabel"), label, rule.Name), rule);
                        }
                    }
                }

                var justTheLabels = clauseLabels.Select(x => x.Key);
                // If any clause has a label they all must have labels
                if (justTheLabels.Any(x => x is string) && justTheLabels.Any(x => x is null))
                {
                    yield return new Violation(string.Format(Strings.Get("Err_ClauseMissingLabels"), rule.Name), rule);
                }
                // If the clause has an expression it may not have any null labels
                if (rule.Expression != null && justTheLabels.Any(x => x is null))
                {
                    yield return new Violation(string.Format(Strings.Get("Err_ClauseExpressionButMissingLabels"), rule.Name), rule);
                }
            }
        }

        /// <summary>
        /// Determine if a Clause is true or false
        /// </summary>
        /// <param name="clause">The Clause to Analyze</param>
        /// <param name="state1">The first object state</param>
        /// <param name="state2">The second object state</param>
        /// <returns>If the Clause is true</returns>
        public bool AnalyzeClause(Clause clause, object? state1 = null, object? state2 = null)
        {
            if (clause == null)
            {
                return false;
            }

            try
            {
                var res = InnerAnalyzer(clause, state1, state2);
                return clause.Invert ? !res : res;
            }
            catch (Exception e)
            {
                Log.Debug(e, $"Hit while parsing {JsonConvert.SerializeObject(clause)} onto ({JsonConvert.SerializeObject(state1)},{JsonConvert.SerializeObject(state2)})");
            }
            return false;


            bool InnerAnalyzer(Clause clause, object? state1 = null, object? state2 = null)
            {
                if (clause.Field is string)
                {
                    state2 = GetValueByPropertyString(state2, clause.Field);
                    state1 = GetValueByPropertyString(state1, clause.Field);
                }

                var typeHolder = state1 ?? state2;

                (var stateOneList, var stateOneDict) = ObjectToValues(state1);
                (var stateTwoList, var stateTwoDict) = ObjectToValues(state2);

                var valsToCheck = stateOneList.Union(stateTwoList);
                var dictToCheck = stateOneDict.Union(stateTwoDict);

                switch (clause.Operation)
                {
                    case OPERATION.EQ:
                        return clause.Data is List<string> EqualsData && EqualsData.Intersect(valsToCheck).Any();

                    case OPERATION.NEQ:
                        return clause.Data is List<string> NotEqualsData && !NotEqualsData.Intersect(valsToCheck).Any();

                    // If *every* entry of the clause data is matched
                    case OPERATION.CONTAINS:
                        if (dictToCheck.Any())
                        {
                            if (clause.DictData is List<KeyValuePair<string, string>> ContainsData
                                    && ContainsData.All(y => dictToCheck.Any((x) => x.Key == y.Key && x.Value == y.Value)))
                            {
                                return true;
                            }
                        }
                        else if (valsToCheck.Any())
                        {
                            if (clause.Data is List<string> ContainsDataList)
                            {
                                // If we are dealing with an array on the object side
                                if (typeHolder is List<string>)
                                {
                                    if (ContainsDataList.All(x => valsToCheck.Contains(x)))
                                    {
                                        return true;
                                    }
                                }
                                // If we are dealing with a single string we do a .Contains instead
                                else if (typeHolder is string)
                                {
                                    if (clause.Data.All(x => valsToCheck.First()?.Contains(x) ?? false))
                                    {
                                        return true;
                                    }
                                }
                                // If we are dealing with a flags Enum we can select the appropriate flag
                                else if (typeHolder?.GetType().IsDefined(typeof(FlagsAttribute), false) is true)
                                {
                                    var enums = new List<Enum>();
                                    foreach (var datum in clause.Data ?? new List<string>())
                                    {
                                        if (Enum.TryParse(typeHolder.GetType(), datum, out object result))
                                        {
                                            if (!(state1 is Enum enum1 && enum1.HasFlag((Enum)result)))
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                        return false;

                    // If *any* entry of the clause data is matched
                    case OPERATION.CONTAINS_ANY:
                        if (dictToCheck.Any())
                        {
                            if (clause.DictData is List<KeyValuePair<string, string>> ContainsData)
                            {
                                foreach (KeyValuePair<string, string> value in ContainsData)
                                {
                                    if (dictToCheck.Any(x => x.Key == value.Key && x.Value == value.Value))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        else if (valsToCheck.Any())
                        {
                            if (clause.Data is List<string> ContainsDataList)
                            {
                                if (typeHolder is List<string>)
                                {
                                    if (ContainsDataList.Any(x => valsToCheck.Contains(x)))
                                    {
                                        return true;
                                    }
                                }
                                // If we are dealing with a single string we do a .Contains instead
                                else if (typeHolder is string)
                                {
                                    if (clause.Data.Any(x => valsToCheck.First()?.Contains(x) ?? false))
                                    {
                                        return true;
                                    }
                                }

                                else if (typeHolder?.GetType().IsDefined(typeof(FlagsAttribute), false) is true)
                                {
                                    var enums = new List<Enum>();
                                    foreach (var datum in clause.Data ?? new List<string>())
                                    {
                                        if (Enum.TryParse(typeHolder.GetType(), datum, out object result))
                                        {
                                            if (state1 is Enum enum1 && enum1.HasFlag((Enum)result))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return false;

                    // If any of the data values are greater than the first provided clause value We
                    // ignore all other clause values
                    case OPERATION.GT:
                        foreach (var val in valsToCheck)
                        {
                            if (int.TryParse(val, out int valToCheck)
                                    && int.TryParse(clause.Data?[0], out int dataValue)
                                    && valToCheck > dataValue)
                            {
                                return true;
                            }
                        }
                        return false;

                    // If any of the data values are less than the first provided clause value We
                    // ignore all other clause values
                    case OPERATION.LT:
                        foreach (var val in valsToCheck)
                        {
                            if (int.TryParse(val, out int valToCheck)
                                    && int.TryParse(clause.Data?[0], out int dataValue)
                                    && valToCheck < dataValue)
                            {
                                return true;
                            }
                        }
                        return false;

                    // If any of the regexes match any of the values
                    case OPERATION.REGEX:
                        if (clause.Data is List<string> RegexList && RegexList.Count > 0)
                        {
                            var built = string.Join('|', RegexList);

                            var regex = StringToRegex(built);
                            
                            if (regex != null && valsToCheck.Any(x => regex.IsMatch(x)))
                            {
                                return true;
                            }
                        }

                        Regex? StringToRegex(string built)
                        {
                            if (!RegexCache.ContainsKey(built))
                            {
                                try
                                {
                                    RegexCache.TryAdd(built, new Regex(built, RegexOptions.Compiled));
                                }
                                catch (ArgumentException)
                                {
                                    Log.Warning("InvalidArgumentException when analyzing clause {0}. Regex {1} is invalid and will be skipped.", clause.Label, built);
                                    RegexCache.TryAdd(built, null);
                                }
                            }
                            return RegexCache[built];
                        }

                        return false;

                    // Ignores provided data. Checks if the named property has changed.
                    case OPERATION.WAS_MODIFIED:
                        var compareLogic = new CompareLogic();

                        var comparisonResult = compareLogic.Compare(state1, state2);

                        return !comparisonResult.AreEqual;

                    // Ends with any of the provided data
                    case OPERATION.ENDS_WITH:
                        return clause.Data is List<string> EndsWithData
                                && valsToCheck.Any(x => EndsWithData.Any(y => x is string
                                    && x.EndsWith(y, StringComparison.CurrentCulture)));

                    // Starts with any of the provided data
                    case OPERATION.STARTS_WITH:
                        if (clause.Data is List<string> StartsWithData
                                && valsToCheck.Any(x => StartsWithData.Any(y => x is string
                                && x.StartsWith(y, StringComparison.CurrentCulture))))
                        {
                            return true;
                        }
                        return false;

                    case OPERATION.IS_NULL:
                        if (valsToCheck.Count(x => x is null) == valsToCheck.Count())
                        {
                            return true;
                        }
                        return false;

                    case OPERATION.IS_TRUE:
                        foreach (var valToCheck in valsToCheck)
                        {
                            if (bool.TryParse(valToCheck, out bool result) && result)
                            {
                                return true;
                            }
                        }
                        return false;

                    case OPERATION.IS_BEFORE:
                        var valDateTimes = new List<DateTime>();
                        foreach (var valToCheck in valsToCheck)
                        {
                            if (DateTime.TryParse(valToCheck, out DateTime result))
                            {
                                valDateTimes.Add(result);
                            }
                        }
                        foreach (var data in clause.Data ?? new List<string>())
                        {
                            if (DateTime.TryParse(data, out DateTime result) && valDateTimes.Any(x => x.CompareTo(result) < 0))
                            {
                                return true;
                            }
                        }
                        return false;

                    case OPERATION.IS_AFTER:
                        valDateTimes = new List<DateTime>();
                        foreach (var valToCheck in valsToCheck)
                        {
                            if (DateTime.TryParse(valToCheck, out DateTime result))
                            {
                                valDateTimes.Add(result);
                            }
                        }
                        foreach (var data in clause.Data ?? new List<string>())
                        {
                            if (DateTime.TryParse(data, out DateTime result) && valDateTimes.Any(x => x.CompareTo(result) > 0))
                            {
                                return true;
                            }
                        }
                        return false;

                    case OPERATION.IS_EXPIRED:
                        if (state1 is DateTime dateTime1 && dateTime1.CompareTo(DateTime.Now) < 0)
                        {
                            return true;
                        }
                        if (state2 is DateTime dateTime2 && dateTime2.CompareTo(DateTime.Now) < 0)
                        {
                            return true;
                        }
                        return false;

                    case OPERATION.CONTAINS_KEY:
                        return dictToCheck.Any(x => clause.Data.Any(y => x.Key == y));

                    case OPERATION.CUSTOM:
                        foreach (var del in CustomOperationDelegates)
                        {
                            var res = del?.Invoke(clause, valsToCheck, dictToCheck, state1, state2);
                            if (res.HasValue && res.Value.Applies)
                            {
                                return res.Value.Result;
                            }
                        }
                        Log.Debug("Custom operation hit but delegate for {0} isn't set.", clause.CustomOperation);
                        return false;

                    default:
                        Log.Debug("Unimplemented operation {0}", clause.Operation);
                        throw new NotImplementedException($"Unimplemented operation {clause.Operation}");
                }

            }
        }

        private static int FindMatchingParen(string[] splits, int startingIndex)
        {
            int foundStarts = 0;
            int foundEnds = 0;
            for (int i = startingIndex; i < splits.Length; i++)
            {
                foundStarts += splits[i].Count(x => x.Equals('('));
                foundEnds += splits[i].Count(x => x.Equals(')'));

                if (foundStarts <= foundEnds)
                {
                    return i;
                }
            }

            return splits.Length - 1;
        }

        /// <summary>
        /// Gets the object value stored at the field or property named by the string. Property tried first.  Returns null if none found.
        /// </summary>
        /// <param name="obj">The target object</param>
        /// <param name="propertyName">The Property or Field name</param>
        /// <returns>The object at that Name or null</returns>
        public static object? GetValueByPropertyOrFieldName(object? obj, string? propertyName) => obj?.GetType().GetProperty(propertyName ?? string.Empty)?.GetValue(obj) ?? obj?.GetType().GetField(propertyName ?? string.Empty)?.GetValue(obj);

        private (List<string>, List<KeyValuePair<string, string>>) ObjectToValues(object? obj)
        {
            var valsToCheck = new List<string>();
            var dictToCheck = new List<KeyValuePair<string, string>>();
            if (obj != null)
            {
                try
                {
                    if (obj is List<string> stringList)
                    {
                        valsToCheck.AddRange(stringList);
                    }
                    else if (obj is Dictionary<string, string> dictString)
                    {
                        dictToCheck = dictString.ToList();
                    }
                    else if (obj is Dictionary<string, List<string>> dict)
                    {
                        dictToCheck = new List<KeyValuePair<string, string>>();
                        foreach (var list in dict.ToList())
                        {
                            foreach (var entry in list.Value)
                            {
                                dictToCheck.Add(new KeyValuePair<string, string>(list.Key, entry));
                            }
                        }
                    }
                    else if (obj is List<KeyValuePair<string, string>> listKvp)
                    {
                        dictToCheck = listKvp;
                    }
                    else
                    {
                        var found = false;
                        foreach (var del in CustomObjectToValuesDelegates)
                        {
                            var res = del?.Invoke(obj);
                            if (res.HasValue && res.Value.Processed)
                            {
                                found = true;
                                (valsToCheck, dictToCheck) = (res.Value.valsExtracted.ToList(), res.Value.dictExtracted.ToList());
                                break;
                            }
                        }
                        if (!found)
                        {
                            var val = obj?.ToString();
                            if (!string.IsNullOrEmpty(val))
                            {
                                valsToCheck.Add(val);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Log.Debug("Failed to Turn Obect into Values");
                }
            }

            return (valsToCheck, dictToCheck);
        }

        private static bool Operate(BOOL_OPERATOR Operator, bool first, bool second)
        {
            return Operator switch
            {
                BOOL_OPERATOR.AND => first && second,
                BOOL_OPERATOR.OR => first || second,
                BOOL_OPERATOR.XOR => first ^ second,
                BOOL_OPERATOR.NAND => !(first && second),
                BOOL_OPERATOR.NOR => !(first || second),
                BOOL_OPERATOR.NOT => !first,
                _ => false
            };
        }

        private bool Evaluate(string[] splits, List<Clause> Clauses, object? state1, object? state2)
        {
            bool current = false;

            var invertNextStatement = false;
            var operatorExpected = false;

            BOOL_OPERATOR Operator = BOOL_OPERATOR.OR;

            var updated_i = 0;

            for (int i = 0; i < splits.Length; i = updated_i)
            {
                if (operatorExpected)
                {
                    Operator = (BOOL_OPERATOR)Enum.Parse(typeof(BOOL_OPERATOR), splits[i]);
                    operatorExpected = false;
                    updated_i = i + 1;
                }
                else if (splits[i].StartsWith("("))
                {
                    //Get the substring closing this paren
                    var matchingParen = FindMatchingParen(splits, i);

                    // First remove the parenthesis from the beginning and end
                    splits[i] = splits[i][1..];
                    splits[matchingParen] = splits[matchingParen][0..^1];

                    var (CanShortcut, Value) = TryShortcut(current, Operator);

                    if (CanShortcut)
                    {
                        current = Value;
                    }
                    else
                    {
                        // Recursively evaluate the contents of the parentheses
                        var next = Evaluate(splits[i..(matchingParen + 1)], Clauses, state1, state2);

                        next = invertNextStatement ? !next : next;

                        current = Operate(Operator, current, next);
                    }

                    updated_i = matchingParen + 1;
                    invertNextStatement = false;
                    operatorExpected = true;
                }
                else
                {
                    if (splits[i].Equals(BOOL_OPERATOR.NOT.ToString()))
                    {
                        invertNextStatement = !invertNextStatement;
                        operatorExpected = false;
                    }
                    else
                    {
                        // Ensure we have exactly 1 matching clause defined
                        var res = Clauses.Where(x => x.Label == splits[i].Replace("(", "").Replace(")", ""));
                        if (!(res.Count() == 1))
                        {
                            return false;
                        }

                        var clause = res.First();

                        var shortcut = TryShortcut(current, Operator);

                        if (shortcut.CanShortcut)
                        {
                            current = shortcut.Value;
                        }
                        else
                        {
                            bool next = AnalyzeClause(res.First(), state1, state2);

                            next = invertNextStatement ? !next : next;

                            current = Operate(Operator, current, next);
                        }

                        invertNextStatement = false;
                        operatorExpected = true;
                    }
                    updated_i = i + 1;
                }
            }
            return current;
        }

        /// <summary>
        /// Try to shortcut a boolean operation
        /// </summary>
        /// <param name="current">The current boolean state</param>
        /// <param name="operation">The Operation</param>
        /// <returns>(If you can use a shortcut, the result of the shortcut)</returns>
        public static (bool CanShortcut, bool Value) TryShortcut(bool current, BOOL_OPERATOR operation)
        {
            // If either argument of an AND statement is false, or either argument of a
            // NOR statement is true, the result is always false and we can optimize
            // away evaluation of next
            if ((operation == BOOL_OPERATOR.AND && !current) ||
                (operation == BOOL_OPERATOR.NOR && current))
            {
                return (true, false);
            }
            // If either argument of an NAND statement is false, or either argument of
            // an OR statement is true, the result is always true and we can optimize
            // away evaluation of next
            if ((operation == BOOL_OPERATOR.OR && current) ||
                (operation == BOOL_OPERATOR.NAND && !current))
            {
                return (true, true);
            }
            return (false, false);
        }
    }
}