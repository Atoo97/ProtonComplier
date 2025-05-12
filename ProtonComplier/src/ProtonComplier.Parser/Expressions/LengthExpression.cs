// <copyright file="LengthExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an expression that accesses the length of a list or array-like identifier (e.g., `list.Length`).
    /// </summary>
    public class LengthExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LengthExpression"/> class.
        /// </summary>
        /// <param name="operand">The operand identifier expression.</param>
        /// <param name="period">The token representing the period (`.`).</param>
        /// <param name="length">The token representing the `Length` keyword.</param>
        /// <exception cref="AnalyzerError">Thrown if any of the tokens are invalid.</exception>
        public LengthExpression(OperandExpression operand, Token period, Token length)
            : base(period)
        {
            if (operand.ParseSymbol.TokenType != TokenType.Identifier)
            {
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, operand.ParseSymbol.TokenValue, operand.ParseSymbol.TokenLine, operand.ParseSymbol.TokenColumn, TokenType.Identifier));
            }

            if (period.TokenType != TokenType.Period)
            {
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, period.TokenValue, period.TokenLine, period.TokenColumn, TokenType.Period));
            }

            if (length.TokenType != TokenType.Length)
            {
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, length.TokenValue, length.TokenLine, length.TokenColumn, TokenType.Length));
            }

            this.Identifier = operand;
            this.Lenght = length;
        }

        /// <summary>
        /// Gets the operand identifier representing the list or array whose length is being accessed.
        /// </summary>
        public OperandExpression Identifier { get; private set; }

        /// <summary>
        /// Gets the length token.
        /// </summary>
        public Token Lenght { get; private set; }

        /// <summary>
        /// Generates a formatted string representation of the length expression for debugging or code visualization.
        /// </summary>
        /// <param name="ident">The indentation level.</param>
        /// <returns>A string representing the expression tree.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 0);
            return $"LengthExpression:\n {this.Identifier.ToCode(ident)}.Length";
        }
    }
}
