// <copyright file="BinaryExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a binary expression in the parse tree, such as "x + 3" or "1 + 3 * (1 - 2)".
    /// </summary>
    public class BinaryExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExpression"/> class.
        /// </summary>
        /// <param name="left">The left expression.</param>
        /// <param name="op">The operator token (e.g., '+', '-', '*').</param>
        /// <param name="remainingTokens">The remaining tokens for the right-hand side of the expression.</param>
        /// <exception cref="AnalyzerError">Thrown when the right-hand side is missing or improperly formed.</exception>
        public BinaryExpression(Expression left, OperatorExpression op, List<Token> remainingTokens)
            : base(left.ParseSymbol)
        {
            this.Left = left;
            this.Operator = op;

            if (remainingTokens == null || remainingTokens.Count == 0)
            {
                // Generate error message
                throw new AnalyzerError(
                    "124",
                    string.Format(MessageRegistry.GetMessage(124).Text, op.ParseSymbol.TokenLine + 1, op.ParseSymbol.TokenColumn + 1));
            }

            this.Right = ExpressionParserHelper.ParseExpression(remainingTokens);
        }

        /// <summary>
        /// Gets the left-hand side expression of the binary expression.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// Gets the operator in the binary expression (e.g., '+', '-', '*').
        /// </summary>
        public OperatorExpression Operator { get; }

        /// <summary>
        /// Gets the right-hand side expression of the binary expression.
        /// </summary>
        public Expression Right { get; private set; }

        /// <summary>
        /// Generates a human-readable string representation of the binary expression, formatted with indentation.
        /// </summary>
        /// <param name="ident">The indentation level.</param>
        /// <returns>A formatted string representation of the binary expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4); // 4 spaces per indent level

            return $"BinaryExpression:\n" +
                   $"{indentLine}├─ LeftOperand: {this.Left.ToCode(ident + 1)}\n" +
                   $"{indentLine}├─ Operator: {this.Operator.ToCode(ident + 1)}\n" +
                   $"{indentLine}└─ RightOperand: {this.Right.ToCode(ident + 1)}";
        }

        /// <summary>
        /// Sets the right-hand side expression explicitly.
        /// </summary>
        /// <param name="right">The right-hand side expression to assign.</param>
        public void SetRight(Expression right)
        {
            this.Right = right;
        }
    }
}
