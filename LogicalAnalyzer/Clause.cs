// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using System.Collections.Generic;

namespace Microsoft.CST.LogicalAnalyzer
{
    /// <summary>
    ///     Clauses contain an Operation and associated data
    /// </summary>
    public class Clause
    {
        public Clause(string Field, OPERATION Operation)
        {
            this.Field = Field;
            this.Operation = Operation;
        }

        public Clause(OPERATION Operation)
        {
            this.Operation = Operation;
        }

        /// <summary>
        ///     A list of strings passed to the operation
        /// </summary>
        public List<string>? Data { get; set; }
        /// <summary>
        ///     A dictionary of strings passed to the operation
        /// </summary>
        public List<KeyValuePair<string, string>>? DictData { get; set; }
        /// <summary>
        ///     Which field or property of the Target should this Clause apply to?
        ///         null is wildcard
        /// </summary>
        public string? Field { get; set; }
        /// <summary>
        ///     The Label used for the Boolean Expression in the Rule containing this Clause
        /// </summary>
        public string? Label { get; set; }
        /// <summary>
        ///     The Operation to perform
        /// </summary>
        public OPERATION Operation { get; set; }
        /// <summary>
        ///     A string indicating what custom operation should be performed, if Operation = Custom
        /// </summary>
        public string? CustomOperation { get; set; }
    }
}