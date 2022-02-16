﻿// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using Microsoft.CST.OAT.Operations;
using Microsoft.CST.OAT.Utils;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.CST.OAT
{
    /// <summary>
    ///     This is the core engine of OAT
    /// </summary>
    public class Analyzer
    {
        /// <summary>
        ///     The constructor for Analyzer takes no arguments.
        /// </summary>
        public Analyzer(OAT.AnalyzerOptions? analyzerOptions = null)
        {
            Options = analyzerOptions ?? new AnalyzerOptions();
            SetOperation(new ContainsOperation(this));
            SetOperation(new ContainsAnyOperation(this));
            SetOperation(new ContainsKeyOperation(this));
            SetOperation(new EqualsOperation(this));
            SetOperation(new EndsWithOperation(this));
            SetOperation(new GreaterThanOperation(this));
            SetOperation(new IsAfterOperation(this));
            SetOperation(new IsBeforeOperation(this));
            SetOperation(new IsExpiredOperation(this));
            SetOperation(new IsNullOperation(this));
            SetOperation(new IsTrueOperation(this));
            SetOperation(new LessThanOperation(this));
            SetOperation(new RegexOperation(this));
            SetOperation(new StartsWithOperation(this));
            SetOperation(new WasModifiedOperation(this));
            SetOperation(new NoOperation(this));
            if (Options.RunScripts)
            {
                try
                {
                    Type? scriptOpType = Assembly.Load("OAT.Scripting").GetTypes().Where(t => t.FullName == "Microsoft.CST.OAT.Operations.ScriptOperation").FirstOrDefault();
                    if (scriptOpType is not null)
                    {
                        if (scriptOpType.GetConstructor(new Type[] { typeof(Analyzer) })?.Invoke(new Analyzer[] { this }) is OatOperation reflectedScriptOperation)
                        {
                            SetOperation(reflectedScriptOperation);
                        }
                        else
                        {
                            Log.Error(Strings.Get("Err_ErrorLoadingScriptingAssembly"));
                        }
                    }
                    else
                    {
                        Log.Error(Strings.Get("Err_ErrorLoadingScriptingAssembly"));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(Strings.Get("Err_ExceptionLoadingScriptingAssembly_{0}"),ex.Message);
                }
            }
            else
            {
                SetOperation(new ScriptsDisabledOperation(this));
            }
        }

        /// <summary>
        ///     This delegate is for turning complex objects like dictionaries that the Analyzer doesn't
        ///     natively support into a dictionary or list of strings that OAT can use for default operations
        /// </summary>
        /// <param name="obj"> Target object </param>
        /// <returns>
        ///     (If the object was parsed, A list of Strings that were extracted, A List of KVP that were extracted)
        /// </returns>
        public delegate (bool Processed, IEnumerable<string> valsExtracted, IEnumerable<KeyValuePair<string, string>> dictExtracted) ObjectToValuesDelegate(object? obj);

        /// <summary>
        ///     This delegate is for iterating into complex objects like dictionaries that the Analyzer
        ///     doesn't natively understand
        /// </summary>
        /// <param name="obj"> Target object </param>
        /// <param name="index"> String based index into the object </param>
        /// <returns> (If we successfully extracted, The extraction result) </returns>
        public delegate (bool Processed, object? Result) PropertyExtractionDelegate(object? obj, string index);

        /// <summary>
        ///     The ObjectToValuesDelegates that will be used in order of attempt. Once successful the others
        ///     won't be run.
        /// </summary>
        public List<ObjectToValuesDelegate> CustomObjectToValuesDelegates { get; set; } = new List<ObjectToValuesDelegate>();

        /// <summary>
        ///     The PropertyExtractionDelegates that will be used in order of attempt. Once successful the
        ///     others won't be run.
        /// </summary>
        public List<PropertyExtractionDelegate> CustomPropertyExtractionDelegates { get; set; } = new List<PropertyExtractionDelegate>();

        /// <summary>
        ///     The options for the Analyzer
        /// </summary>
        public AnalyzerOptions Options { get; } = new AnalyzerOptions();

        /// <summary>
        ///     Check if a boolean operation can be shortcut and provide the shorcut if so.
        /// </summary>
        /// <param name="current"> The current boolean state </param>
        /// <param name="operation"> The Operation </param>
        /// <returns> (If you can use a shortcut, the result of the shortcut) </returns>
        public static (bool CanShortcut, bool Value) TryShortcut(bool current, BoolOperator operation)
        {
            // If either argument of an AND statement is false, or either argument of a NOR statement is true,
            // the result is always false and we can optimize away evaluation of next
            if ((operation == BoolOperator.AND && !current) ||
                (operation == BoolOperator.NOR && current))
            {
                return (true, false);
            }
            // If either argument of an NAND statement is false, or either argument of an OR statement is
            // true, the result is always true and we can optimize away evaluation of next
            if ((operation == BoolOperator.OR && current) ||
                (operation == BoolOperator.NAND && !current))
            {
                return (true, true);
            }
            return (false, false);
        }

        /// <summary>
        ///     Which rules apply to this object given up to two states?
        /// </summary>
        /// <param name="rules"> The rules to apply </param>
        /// <param name="state1"> The first state </param>
        /// <param name="state2"> The second state </param>
        /// <returns> A Stack of Rules which apply </returns>
        public IEnumerable<Rule> Analyze(IEnumerable<Rule> rules, object? state1 = null, object? state2 = null)
        {
            var results = new ConcurrentStack<Rule>();

            if (Options.Parallel)
            {
                Parallel.ForEach(rules, rule =>
                {
                    if (Applies(rule, state1, state2))
                    {
                        results.Push(rule);
                    }
                });
                foreach(var result in results) { yield return result; }
            }
            else
            {
                foreach(var rule in rules)
                {
                    if (Applies(rule, state1, state2))
                    {
                        yield return rule;
                    }
                }
            }
        }

        /// <summary>
        ///     Determine if a Clause is true or false
        /// </summary>
        /// <param name="clause"> The Clause to Analyze </param>
        /// <param name="state1"> The first object state </param>
        /// <param name="state2"> The second object state </param>
        /// <returns> If the Clause is true </returns>
        public bool AnalyzeClause(Clause clause, object? state1 = null, object? state2 = null)
        {
            if (clause == null)
            {
                return false;
            }

            try
            {
                var res = GetClauseCapture(clause, state1, state2);
                return res.Result;
            }
            catch (Exception e)
            {
                Log.Debug(e, $"Hit while parsing {clause.Label} onto ({state1},{state2})");
            }
            return false;
        }

        /// <summary>
        ///     Does the rule apply to the object?
        /// </summary>
        /// <param name="rule"> The Rule to apply </param>
        /// <param name="state1"> The first state of the object </param>
        /// <param name="state2"> The second state of the object </param>
        /// <returns> True if the rule applies </returns>
        public bool Applies(Rule rule, object? state1 = null, object? state2 = null)
        {
            if (rule != null)
            {
                var sample = state1 is null ? state2 : state1;

                // Does the name of this class match the Target in the rule? Or has no target been specified
                // (match all)
                if (rule.Target is null || (sample?.GetType().GetFriendlyName().Equals(rule.Target, StringComparison.InvariantCultureIgnoreCase) ?? true))
                {
                    // If the expression is null the default is that all clauses must be true If we have no
                    // clauses .All will still match
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
                        var (Success, Capture) = Evaluate(rule.Expression.Split(' '), rule.Clauses, state1, state2);
                        if (Success)
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
        ///     Clear all the set delegates
        /// </summary>
        public void ClearDelegates()
        {
            Delegates.Clear();
        }

        /// <summary>
        ///     Verifies the provided rule and provides a list of issues with the rules.
        /// </summary>
        /// <param name="rule"> A Rule. </param>
        /// <returns> Enumerable of issues with the Rule. </returns>
        public IEnumerable<Violation> EnumerateRuleIssues(Rule rule)
        {
            var clauseLabels = rule.Clauses.GroupBy(x => x.Label);

            // If clauses have duplicate names
            foreach (var duplicateClause in clauseLabels.Where(x => x.Key != null && x.Count() > 1))
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseDuplicateName"), rule.Name, duplicateClause.Key ?? string.Empty), rule, duplicateClause.AsEnumerable().ToArray());
            }

            // If clause label contains illegal characters
            foreach (var clause in rule.Clauses)
            {
                if (clause.Label is string label)
                {
                    if (label.Contains(' ') || label.Contains('(') || label.Contains(')'))
                    {
                        yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidLabel"), rule.Name, label), rule, clause);
                    }
                }
                if (Delegates.ContainsKey(clause.Key))
                {
                    foreach (var violation in Delegates[clause.Key].ValidationDelegate.Invoke(rule, clause))
                    {
                        yield return violation;
                    }
                }
                else
                {
                    yield return new Violation(string.Format(Strings.Get("Err_ClauseUnsuppportedOperator_{0}{1}{2}{3}"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), clause.Operation.ToString(), clause.CustomOperation), rule, clause);
                }
                if (clause.Script is ScriptData clauseScript && !string.IsNullOrEmpty(clauseScript.Code))
                {
                    if (!Options.RunScripts)
                    {
                        yield return new Violation(string.Format(Strings.Get("Err_ScriptingDisabled_{0}{1}"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
                    }
                }
            }

            if (rule.Expression is string expression)
            {
                // Are parenthesis balanced?
                // Are spaces correct?
                // Are all variables defined by clauses?
                // Are variables and operators alternating?
                var splits = expression.Split(' ');
                var foundStarts = 0;
                var foundEnds = 0;
                var expectingOperator = false;
                for (var i = 0; i < splits.Length; i++)
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

                        for (var j = 0; j < splits[i].Length; j++)
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
                                // If we've set a close this is invalid because we can't have other characters
                                // after it
                                if (lastClose != -1)
                                {
                                    yield return new Violation(string.Format(Strings.Get("Err_ClauseCharactersAfterClosedParentheses"), expression, rule.Name, splits[i]), rule);
                                }
                            }
                        }

                        var variable = splits[i].Replace("(", "").Replace(")", "");

                        if (variable == "NOT")
                        {
                            if (splits[i].Contains(')'))
                            {
                                yield return new Violation(string.Format(Strings.Get("Err_ClauseCloseParenthesesInNot"), expression, rule.Name, splits[i]), rule);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(variable) || (!rule.Clauses.Any(x => x.Label == variable) && !(int.TryParse(variable, out var result) && result < rule.Clauses.Count)))
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
                        if (!Enum.TryParse<BoolOperator>(splits[i], out var op))
                        {
                            yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidOperator"), expression, rule.Name, splits[i]), rule);
                        }
                        // We don't allow NOT operators to modify other Operators, so we can't allow NOT here
                        else
                        {
                            if (op is BoolOperator boolOp && boolOp == BoolOperator.NOT)
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
        }

        /// <summary>
        ///     Verifies the provided rules and provides a list of issues with the rules.
        /// </summary>
        /// <param name="rules"> Enumerable of Rules. </param>
        /// <returns> Enumerable of issues with the rules. </returns>
        public IEnumerable<Violation> EnumerateRuleIssues(IEnumerable<Rule> rules)
        {
            if (!Strings.IsLoaded)
            {
                Strings.Setup();
            }
            foreach (var rule in rules ?? Array.Empty<Rule>())
            {
                foreach (var issue in EnumerateRuleIssues(rule))
                {
                    yield return issue;
                }
            }
        }

        /// <summary>
        ///     Checks if the Rule matches and obtains its Capture
        /// </summary>
        /// <param name="rule"> The Rule to test </param>
        /// <param name="state1"> object state1 </param>
        /// <param name="state2"> object state2 </param>
        /// <returns> </returns>
        public (bool RuleMatches, RuleCapture? Result) GetCapture(Rule rule, object? state1 = null, object? state2 = null)
        {
            if (rule != null)
            {
                var ruleCapture = new RuleCapture(rule, new List<ClauseCapture>());
                var sample = state1 is null ? state2 : state1;

                // Does the name of this class match the Target in the rule? Or has no target been specified
                // (match all)
                if (rule.Target is null || (sample?.GetType().Name.Equals(rule.Target, StringComparison.InvariantCultureIgnoreCase) ?? true))
                {
                    // If the expression is null the default is that all clauses must be true If we have no
                    // clauses .All will still match
                    if (rule.Expression is null)
                    {
                        foreach (var clause in rule.Clauses)
                        {
                            var res = GetClauseCapture(clause, state1, state2, ruleCapture.Captures);
                            if (res.Result)
                            {
                                if (res.Capture != null)
                                {
                                    ruleCapture.Captures.Add(res.Capture);
                                }
                            }
                            else
                            {
                                return (false, null);
                            }
                        }
                        return (true, ruleCapture);
                    }
                    // Otherwise we evaluate the expression
                    else
                    {
                        (var ExpressionMatches, var Captures) = Evaluate(rule.Expression.Split(' '), rule.Clauses, state1, state2, ruleCapture.Captures);
                        if (ExpressionMatches)
                        {
                            ruleCapture.Captures.AddRange(Captures ?? new List<ClauseCapture>());
                            return (true, ruleCapture);
                        }
                    }
                }
            }
            return (false, null);
        }

        /// <summary>
        ///     Get the RuleCaptures for the List of rules as applied to the objects
        /// </summary>
        /// <param name="rules"> List of Rules to run </param>
        /// <param name="state1"> First state of object </param>
        /// <param name="state2"> Second state of object </param>
        /// <returns> </returns>
        public IEnumerable<RuleCapture> GetCaptures(IEnumerable<Rule> rules, object? state1 = null, object? state2 = null)
        {
            var results = new ConcurrentStack<RuleCapture>();
            if (Options.Parallel)
            {
                Parallel.ForEach(rules, rule =>
                {
                    var (RuleMatches, Result) = GetCapture(rule, state1, state2);
                    if (RuleMatches && Result != null)
                    {
                        results.Push(Result);
                    }
                });
                foreach (var entry in results) { yield return entry; }
            }
            else
            {
                foreach(var rule in rules)
                {
                    var (RuleMatches, Result) = GetCapture(rule, state1, state2);
                    if (RuleMatches && Result != null)
                    {
                        yield return Result;
                    }
                }
            }
        }

        /// <summary>
        ///     Get the Tags which apply to the object given the Rules
        /// </summary>
        /// <param name="rules"> The Rules to apply </param>
        /// <param name="state1"> The first state of the object </param>
        /// <param name="state2"> The second state of the object </param>
        /// <returns> </returns>
        public string[] GetTags(IEnumerable<Rule> rules, object? state1 = null, object? state2 = null)
        {
            var tags = new ConcurrentDictionary<string, byte>();

            if (Options.Parallel)
            {
                Parallel.ForEach(rules, rule =>
                {
                    PushRuleTagsIfApplies(rule);
                });
            }
            else
            {
                foreach(var rule in rules)
                {
                    PushRuleTagsIfApplies(rule);
                }
            }

            return tags.Keys.ToArray();

            void PushRuleTagsIfApplies(Rule rule)
            {
                // If there are no tags, or all of the tags are already in the tags we've found skip otherwise apply.
                if ((!rule.Tags.Any() || !rule.Tags.All(x => tags.Keys.Any(y => y == x))) && Applies(rule, state1, state2))
                {
                    foreach (var tag in rule.Tags)
                    {
                        tags.TryAdd(tag, 0);
                    }
                }
            }
        }

        /// <summary>
        ///     Extracts a value stored at the specified path inside an object. Can crawl into Lists and
        ///     Dictionaries of strings and return any top-level object.
        /// </summary>
        /// <param name="targetObject"> The object to parse </param>
        /// <param name="pathToProperty"> The path of the property to fetch </param>
        /// <returns> The object found </returns>
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
                var value = Helpers.GetValueByPropertyOrFieldName(targetObject, pathPortions[0]);

                // For the rest of the path we walk each portion to get the next object
                for (var pathPortionIndex = 1; pathPortionIndex < pathPortions.Length; pathPortionIndex++)
                {
                    if (value == null) { break; }

                    switch (value)
                    {
                        case Dictionary<string, string> stringDict:
                            if (stringDict.TryGetValue(pathPortions[pathPortionIndex], out var stringValue))
                            {
                                value = stringValue;
                            }
                            else
                            {
                                value = null;
                            }
                            break;

                        case List<string> stringList:
                            if (int.TryParse(pathPortions[pathPortionIndex], out var ArrayIndex) && stringList.Count > ArrayIndex)
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
                                value = Helpers.GetValueByPropertyOrFieldName(value, pathPortions[pathPortionIndex]);
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
        ///     Determines if there are any problems with the provided rule.
        /// </summary>
        /// <param name="rule"> The rule to parse. </param>
        /// <returns> True if there are no issues. </returns>
        public bool IsRuleValid(Rule rule) => !EnumerateRuleIssues(new Rule[] { rule }).Any();

        /// <summary>
        ///     Extracts string Values from an Object. Will call the custom ObjectToValues delegate.
        /// </summary>
        /// <param name="obj"> </param>
        /// <returns> A tuple of A list of Strings extracted and a List of KVP extracted. </returns>
        public (List<string>, List<KeyValuePair<string, string>>) ObjectToValues(object? obj)
        {
            var valsToCheck = new List<string>();
            // This supports both Dictionaries and Lists of KVP
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
                            if (val is not null)
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

        /// <summary>
        ///     Set the OatOperation which will be trigged by the provided Operation (and when Custom, CustomOperation)
        /// </summary>
        /// <param name="oatOperation"> The OatOperation </param>
        /// <returns> </returns>
        public bool SetOperation(OatOperation oatOperation)
        {
            Delegates[oatOperation.Key] = oatOperation;
            return true;
        }

        private Dictionary<(Operation Operation, string CustomOperation), OatOperation> Delegates { get; } = new Dictionary<(Operation Operation, string CustomOperation), OatOperation>();

        private static int FindMatchingParen(string[] splits, int startingIndex)
        {
            var foundStarts = 0;
            var foundEnds = 0;
            for (var i = startingIndex; i < splits.Length; i++)
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

        private static bool Operate(BoolOperator Operator, bool first, bool second)
        {
            return Operator switch
            {
                BoolOperator.AND => first && second,
                BoolOperator.OR => first || second,
                BoolOperator.XOR => first ^ second,
                BoolOperator.NAND => !(first && second),
                BoolOperator.NOR => !(first || second),
                BoolOperator.NOT => !first,
                _ => false
            };
        }

        private (bool Success, List<ClauseCapture>? Capture) Evaluate(string[] splits, List<Clause> Clauses, object? state1, object? state2, IEnumerable<ClauseCapture>? captures = null)
        {
            var current = false;

            var captureOut = new List<ClauseCapture>();

            var invertNextStatement = false;
            var operatorExpected = false;

            var Operator = BoolOperator.OR;

            var updated_i = 0;

            for (var i = 0; i < splits.Length; i = updated_i)
            {
                if (operatorExpected)
                {
                    Operator = (BoolOperator)Enum.Parse(typeof(BoolOperator), splits[i]);
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

                    // One of the labels ahead has a capture, so we can't shortcut
                    if (Clauses.Any(x => x.Capture && splits[i..(matchingParen + 1)].Contains(x.Label)))
                    {
                        current = EvaluateLambda();
                    }
                    else if (TryShortcut(current, Operator) is (bool CanShortcut, bool Value) && CanShortcut)
                    {
                        current = Value;
                    }
                    else
                    {
                        current = EvaluateLambda();
                    }

                    updated_i = matchingParen + 1;
                    invertNextStatement = false;
                    operatorExpected = true;

                    bool EvaluateLambda()
                    {
                        // Recursively evaluate the contents of the parentheses
                        var capturesUnion = captures is null ? captureOut : captureOut.Union(captures);
                        var (Success, Capture) = Evaluate(splits[i..(matchingParen + 1)], Clauses, state1, state2, capturesUnion);

                        if (Success)
                        {
                            captureOut.AddRange(Capture ?? new List<ClauseCapture>());
                        }

                        var next = invertNextStatement ? !Success : Success;

                        return Operate(Operator, current, next);
                    }
                }
                else
                {
                    if (splits[i].Equals(BoolOperator.NOT.ToString()))
                    {
                        invertNextStatement = !invertNextStatement;
                        operatorExpected = false;
                    }
                    else
                    {
                        // Ensure we have exactly 1 matching clause defined
                        var targetLabel = splits[i].Replace("(", "").Replace(")", "");
                        var res = Clauses.Where(x => x.Label == targetLabel);
                        if (res.Count() > 1)
                        {
                            Log.Debug($"Multiple Clauses match the label {res.First().Label} so skipping evaluation of expression.  Run EnumerateRuleIssues to identify rule issues.");
                            return (false, null);
                        }

                        // If we couldn't find a label match fall back to trying to parse this as an index
                        // into clauses
                        if (!res.Any() && int.TryParse(targetLabel, out var result) && Clauses.Count > result)
                        {
                            res = new Clause[] { Clauses[result] };
                        }

                        // To handle the first element the defaults here are `false`, `OR`, which cannot be shortcut.
                        var (CanShortcut, Value) = TryShortcut(current, Operator);

                        if (CanShortcut && !Clauses.Any(x => x.Capture))
                        {
                            current = Value;
                        }
                        else
                        {
                            var captureEnumerable = captures is null ? captureOut : captureOut.Union(captures);
                            var res2 = GetClauseCapture(res.First(), state1, state2, captureEnumerable);

                            if (res2.Result && res2.Capture != null)
                            {
                                captureOut.Add(res2.Capture);
                            }

                            var next = invertNextStatement ? !res2.Result : res2.Result;

                            current = Operate(Operator, current, next);
                        }

                        invertNextStatement = false;
                        operatorExpected = true;
                    }
                    updated_i = i + 1;
                }
            }
            return (current, captureOut);
        }

        private OperationResult GetClauseCapture(Clause clause, object? state1 = null, object? state2 = null, IEnumerable<ClauseCapture>? captures = null)
        {
            if (clause.Field is not null)
            {
                state2 = GetValueByPropertyString(state2, clause.Field);
                state1 = GetValueByPropertyString(state1, clause.Field);
            }

            if (Delegates.ContainsKey(clause.Key))
            {
                return Delegates[clause.Key].OperationDelegate.Invoke(clause, state1, state2, captures);
            }
            else
            {
                return Delegates[(Operation.NoOperation, "")].OperationDelegate.Invoke(clause, state1, state2, captures);
            }
        }
    }
}