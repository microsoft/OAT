// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License.
using System;

namespace Microsoft.CST.OAT
{
    /// <summary>
    /// A violation found when validating rules
    /// </summary>
    public class Violation
    {
        /// <summary>
        ///     The Rule associated with this violation
        /// </summary>
        public Rule rule { get; set; }
        /// <summary>
        ///     The clauses associated with this violation
        /// </summary>
        public Clause[] clauses { get; set; }
        /// <summary>
        ///     The text description of the violation
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Simplest constructor.
        /// 
        /// Arguments may not be null.
        /// </summary>
        /// <param name="description">The description for the violation.</param>
        /// <param name="rule">The Rule the violation references</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null.</exception>
        public Violation(string description, Rule rule) : this(description, rule, Array.Empty<Clause>()) { }

        /// <summary>
        /// Use this constructor if you have only a single clause
        /// 
        /// Arguments may not be null.
        /// </summary>
        /// <param name="description">The description for the violation.</param>
        /// <param name="rule">The Rule the violation references</param>
        /// <param name="clause">The Clause the violation references</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null.</exception>
        public Violation(string description, Rule rule, Clause clause) : this(description, rule, new Clause[] { clause }) { }

        /// <summary>
        /// Use this constructor if you have multiple clauses.
        ///
        /// Arguments may not be null.
        /// </summary>
        /// <param name="description">The description for the violation.</param>
        /// <param name="rule">The Rule the violation references</param>
        /// <param name="clauses">The Clauses the violation references</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null.</exception>
        public Violation(string description, Rule rule, Clause[] clauses)
        {
            this.description = description ?? throw new ArgumentNullException(nameof(description), $"{nameof(description)} may not be null");
            this.rule = rule ?? throw new ArgumentNullException(nameof(rule), $"{nameof(rule)} may not be null");
            this.clauses = clauses ?? throw new ArgumentNullException(nameof(clauses), $"{nameof(clauses)} may not be null");
        }
    }
}