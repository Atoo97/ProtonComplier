// <copyright file="MinExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a Min expression that evaluates the minimum value between two sub-expressions.
    /// </summary>
    public class MinExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinExpression"/> class with the specified left and right expressions.
        /// </summary>
        /// <param name="left">The left-hand expression.</param>
        /// <param name="right">The right-hand expression.</param>
        public MinExpression(Expression left, Expression right)
            : base(left.ParseSymbol)
        {
            this.LeftExpression = left;
            this.RightExpression = right;
        }

        /// <summary>
        /// Gets the left-hand expression of the Max operation.
        /// </summary>
        public Expression LeftExpression { get; private set; }

        /// <summary>
        /// Gets the right-hand expression of the Max operation.
        /// </summary>
        public Expression RightExpression { get; private set; }

        /// <summary>
        /// Returns a formatted string representing the structure of the Min expression.
        /// </summary>
        /// <param name="ident">The indentation level.</param>
        /// <returns>A string that describes the Max expression tree.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 0);
            return $"MinExpression:\n  Left: {this.LeftExpression.ToCode(ident)} , Right: {this.RightExpression.ToCode(ident)}";
        }
    }

}
