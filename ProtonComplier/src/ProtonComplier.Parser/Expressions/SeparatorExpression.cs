// <copyright file="SeparatorExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a separator expression, such as the colon ":" used in various syntax constructs.
    /// </summary>
    public class SeparatorExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeparatorExpression"/> class.
        /// Validates that the provided token is of type "Colon".
        /// </summary>
        /// <param name="variable">The token representing the separator (e.g., ":").</param>
        /// <exception cref="Exception">Thrown if the token is not of type "Colon".</exception>
        public SeparatorExpression(Token variable)
            : base(variable)
        {
            if (variable.TokenType != TokenType.Colon)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, variable!.TokenValue, variable.TokenLine, variable.TokenColumn, TokenType.Colon));
            }
        }

        /// <summary>
        /// Generates the string representation of the separator expression for code generation,
        /// applying the specified indentation.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the separator expression with indentation.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"{indentLine}Separator: ({base.ToCode(ident)})";
        }
    }
}
