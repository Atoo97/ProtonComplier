// <copyright file="Statement.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Statements
{
    using Proton.Parser.Expressions;

    /// <summary>
    /// Represents the base class for all statement nodes in the parse tree.
    /// A statement typically performs an action and does not return a value.
    /// </summary>
    public abstract class Statement : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Statement"/> class using a parsed expression.
        /// </summary>
        /// <param name="expression">The expression containing the parse symbol for this statement.</param>
        protected Statement(Expression expression)
            : base(expression.ParseSymbol!)
        {
        }

        /// <summary>
        /// Converts the statement to its string representation for code generation.
        /// </summary>
        /// <returns>A string representation of the statement prefixed with "State:".</returns>
        public virtual string ToCode()
        {
            return $"Satement:{this.ParseSymbol!.TokenValue}";
        }
    }
}
