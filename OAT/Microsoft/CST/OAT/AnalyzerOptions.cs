// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
namespace Microsoft.CST.OAT
{
    /// <summary>
    /// Holds options to instantiate an Analyzer with
    /// </summary>
    public class AnalyzerOptions
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="runScripts">If running Scripts should be enabled</param>
        /// <param name="parallel">If the analyzer should parallelize analysis</param>
        public AnalyzerOptions(bool runScripts = false, bool parallel = true)
        {
            RunScripts = runScripts;
            Parallel = parallel;
        }

        /// <summary>
        /// If running Scripts is enabled
        /// </summary>
        public bool RunScripts { get; }
        /// <summary>
        /// Should analysis be done using parallelization
        /// </summary>
        public bool Parallel { get; }
    }
}