using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CST.OAT.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.CST.OAT.Operations
{
    /// <summary>
    /// An operation that uses a provided script which is compiled and run.
    /// </summary>
    public class ScriptOperation : OatOperation
    {
        /// <summary>
        /// Create a script Operation that has the provided Assemblies added by reference.
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="assemblies"></param>
        public ScriptOperation(Analyzer analyzer) : base(Operation.Script, analyzer)
        {
            OperationDelegate = ScriptOperationDelegate;
            ValidationDelegate = ScriptOperationValidationDelegate;
        }

        readonly List<PortableExecutableReference>? Assemblies = null;

        internal IEnumerable<Violation> ScriptOperationValidationDelegate(Rule rule, Clause clause)
        {
            if (clause.Script is ScriptData clauseScript)
            {
                List<Violation> issues = new List<Violation>();
                Exception? yieldError = null;
                try
                {
                    var options = ScriptOptions.Default.AddImports("Microsoft.CST.OAT");
                    options = options.AddImports(clauseScript.Imports);
                    options = options.AddReferences(typeof(Analyzer).Assembly);

                    options = options.AddReferences(clauseScript.References.Select(Assembly.Load));
                    var script = CSharpScript.Create<OperationResult>(clauseScript.Code, globalsType: typeof(OperationArguments), options: options);

                    foreach (var issue in script.Compile())
                    {
                        issues.Add(new Violation(issue.GetMessage(), rule, clause));
                    }
                }
                catch (Exception e)
                {
                    yieldError = e;
                }
                if (yieldError != null)
                {
                    yield return new Violation(string.Format(Strings.Get("Err_ClauseInvalidLambda_{0}{1}{2}"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture), yieldError.Message), rule, clause);
                }
                foreach (var issue in issues)
                {
                    yield return issue;
                }
            }
            else
            {
                yield return new Violation(string.Format(Strings.Get("Err_ClauseMissingScript{0}{1}"), rule.Name, clause.Label ?? rule.Clauses.IndexOf(clause).ToString(CultureInfo.InvariantCulture)), rule, clause);
            }
        }

        private Dictionary<ScriptData, Script<OperationResult>?> lambdas { get; } = new Dictionary<ScriptData, Script<OperationResult>?>();

        internal OperationResult ScriptOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            if (clause.Script is ScriptData scriptData)
            {
                if (!lambdas.ContainsKey(clause.Script))
                {
                    try
                    {
                        var options = ScriptOptions.Default.AddImports("Microsoft.CST.OAT");
                        options = options.AddImports(scriptData.Imports);

                        if (Assemblies != null)
                        {
                            options = options.AddReferences(Assemblies);
                        }
                        else
                        {
                            options = options.AddReferences(typeof(Analyzer).Assembly);
                            options = options.AddReferences(scriptData.References.Select(Assembly.Load));
                        }

                        var script = CSharpScript.Create<OperationResult>(scriptData.Code, globalsType: typeof(OperationArguments), options: options);
                        var issues = script.Compile();
                        if (issues.Any())
                        {
                            Log.Debug($"Lambda {scriptData.Code} could not be compiled. ({string.Join("\n", issues)})");
                        }
                        lambdas[scriptData] = script;
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e, $"Lambda {scriptData.Code} could not be compiled.");
                        lambdas[scriptData] = null;
                    }
                }
                try
                {
                    var res = lambdas[scriptData]?.RunAsync(new OperationArguments(clause, state1, state2, captures));
                    return lambdas[scriptData]?.RunAsync(new OperationArguments(clause, state1, state2, captures)).Result.ReturnValue ?? new OperationResult(false, null);
                }
                catch (Exception e)
                {
                    Log.Debug(e, "Found while attempting to execute lambda.");
                }
            }
            return new OperationResult(false, null);
        }
    }
}
