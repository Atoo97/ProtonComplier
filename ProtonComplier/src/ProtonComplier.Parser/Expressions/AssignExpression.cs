// <copyright file="AssignExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an assignment expression, such as the "=" operator used to assign values.
    /// </summary>
    public class AssignExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignExpression"/> class.
        /// Validates that the provided token is of type "Assign".
        /// </summary>
        /// <param name="variable">The token representing the assignment operator ("="). </param>
        /// <exception cref="Exception">Thrown if the token is not of type "Assign".</exception>
        public AssignExpression(Token variable)
            : base(variable)
        {
            if (variable.TokenType != TokenType.Assign)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, variable.TokenValue, variable.TokenLine, variable.TokenColumn, TokenType.Assign));
            }
        }

        /// <summary>
        /// Generates the string representation of the assignment expression for code generation,
        /// including the assignment operator and proper indentation.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the assignment expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"{indentLine}Assignment: {base.ToCode(ident)}";
        }
    }
}
