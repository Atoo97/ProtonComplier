// <copyright file="OptionalExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an optional expression, such as the "Opt" operator used to programming theorems.
    /// </summary>
    public class OptionalExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalExpression"/> class with the given token and expression.
        /// </summary>
        /// <param name="opt">The token representing the optional operator, which must be of type <see cref="TokenType.Options"/>.</param>
        /// <param name="condition">The expression that is condition of expression.</param>
        /// <param name="counter">Indicates the increasing value of the expression.</param>
        /// <exception cref="AnalyzerError">Thrown if the provided token is not of type <see cref="TokenType.Options"/>.</exception>
        public OptionalExpression(Token opt, Expression condition, Expression counter)
            : base(opt)
        {
            if (opt.TokenType != TokenType.Options)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, opt.TokenValue, opt.TokenLine, opt.TokenColumn, TokenType.Options));
            }

            this.LeftExpression = condition;
            this.RightExpression = counter;
        }

        /// <summary>
        /// Gets the right-hand expression of the optional operation.
        /// </summary>
        public Expression RightExpression { get; private set; }

        /// <summary>
        /// Gets the right-hand expression of the optional operation.
        /// </summary>
        public Expression LeftExpression { get; private set; }

        /// <summary>
        /// Generates a formatted code-like string representing this optional expression.
        /// </summary>
        /// <param name="ident">The indentation level used for formatting.</param>
        /// <returns>A string that visually represents the optional expression structure.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"OptionalExpression:\n" +
                  $"{indentLine}├─ Condition: {this.LeftExpression.ToCode(ident + 1)}\n" +
                  $"{indentLine}└─ Counter: {this.RightExpression.ToCode(ident + 1)}";
        }
    }
}
