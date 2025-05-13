// <copyright file="ImplicationExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an implication expression.
    /// </summary>
    public class ImplicationExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicationExpression"/> class.
        /// Validates that the provided token is of type "Implication".
        /// </summary>
        /// <param name="implication">The token representing the implication operator ("->"). </param>
        /// <exception cref="Exception">Thrown if the token is not of type "Assign".</exception>
        public ImplicationExpression(Token implication)
            : base(implication)
        {
            if (implication.TokenType != TokenType.Implication)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, implication.TokenValue, implication.TokenLine, implication.TokenColumn, TokenType.Assign));
            }
        }

        /// <summary>
        /// Generates the string representation of the implication expression for code generation,
        /// including the implication operator and proper indentation.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the implication expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"{indentLine}Implication: {base.ToCode(ident)}";
        }
    }
}
