// <copyright file="PostconditionImplication.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Statements
{
    using System.Text;
    using Proton.Parser.Expressions;
    using ProtonComplier.Parser.Statements;

    /// <summary>
    /// Represents a postcondition implication statement that includes a condition,
    /// an implication expression, and a list of variable initializations to execute
    /// when the implication is satisfied.
    /// </summary>
    public class PostconditionImplication : Statement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostconditionImplication"/> class.
        /// </summary>
        /// <param name="condition">The logical condition to evaluate.</param>
        /// <param name="implication">The implication expression associated with the condition.</param>
        /// <param name="statements">The variable initialization statements to execute if the implication holds.</param>
        public PostconditionImplication(Expression condition, ImplicationExpression implication, List<VariableInitialization> statements)
           : base(implication)
        {
            // Set the left children for the this statement
            this.SetLeft(condition);
            this.Initializations = statements;
        }

        /// <summary>
        /// Gets the list of variable initialization statements.
        /// </summary>
        public List<VariableInitialization> Initializations { get; private set; }

        /// <summary>
        /// Converts the postcondition implication statement to its formatted code string representation.
        /// </summary>
        /// <returns>A string representing the structure of the postcondition implication.</returns>
        public override string ToCode()
        {
            var builder = new StringBuilder();

            builder.AppendLine("\nPostconditionImplication");
            builder.AppendLine(" ├─ Condition:");
            builder.AppendLine($" │  └─ {this.LeftNode!.ToCode(1)}");
            builder.AppendLine($" ├─ Implication: {this.ToCode(0)}");
            builder.AppendLine(" └─ VariableInitialization Statements:");

            for (int i = 0; i < this.Initializations.Count; i++)
            {
                var prefix = i == this.Initializations.Count - 1 ? "    └─" : "    ├─";
                builder.AppendLine($"{prefix} {this.Initializations[i].ToCode(1)}");
            }

            return builder.ToString();
        }
    }
}
