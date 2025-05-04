// <copyright file="LiteralExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an literal expression in the parse tree (e.g., "abc", True/False, -2.34, 'c').
    /// </summary>
    public class LiteralExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralExpression"/> class.
        /// This constructor checks that the token is of a valid literal type (UInt, Integer, Double, Char, Boolean or String).
        /// </summary>
        /// <param name="literalToken">The token representing the literal (UInt, Int, Double, Char, Bool or String).</param>
        /// <exception cref="Exception">Thrown when the token type is not a valid literal type.</exception>
        public LiteralExpression(Token literalToken)
           : base(literalToken)
        {
            if (literalToken.TokenCategory != TokenCategory.Literal)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, literalToken.TokenValue, literalToken.TokenLine, literalToken.TokenColumn, TokenCategory.Literal));
            }
        }

        /// <summary>
        /// Generates the string representation of the literal expression for code generation,
        /// including indentation and formatting.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the literal expression with indentation.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"{indentLine}Literal: ({base.ToCode(ident)})";
        }
    }
}
