// <copyright file="IdentifierExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an identifier expression in the parse tree (e.g., variable names like "_varX").
    /// </summary>
    public class IdentifierExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierExpression"/> class.
        /// </summary>
        /// <param name="variable">The token representing the identifier.</param>
        /// <exception cref="Exception">Thrown if the token type is not <see cref="TokenType.Identifier"/>.</exception>
        public IdentifierExpression(Token variable)
            : base(variable)
        {
            if (variable.TokenType != TokenType.Identifier)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, variable.TokenValue, variable.TokenLine, variable.TokenColumn, TokenType.Identifier));
            }
        }

        /// <summary>
        /// Generates the string representation of the identifier expression for code generation,
        /// including indentation and formatting.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the identifier.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"{indentLine}Variable: {base.ToCode(ident)}";
        }
    }
}
