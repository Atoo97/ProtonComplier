// <copyright file="OperatorExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an operator expression in the parse tree (e.g., "+", "-", "*", "/", "%").
    /// </summary>
    public class OperatorExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorExpression"/> class.
        /// This constructor checks that the token is of a valid operator type (Addition, Subtraction, Multiplication, Division, or Modulus).
        /// </summary>
        /// <param name="operatorToken">The token representing the operator (Addition, Subtraction, Multiplication, Division, or Modulus).</param>
        /// <exception cref="Exception">Thrown when the token type is not a valid operator type.</exception>
        public OperatorExpression(Token operatorToken)
           : base(operatorToken)
        {
            if (operatorToken.TokenCategory != TokenCategory.Operator || operatorToken.TokenType == TokenType.Assign)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, operatorToken.TokenValue, operatorToken.TokenLine, operatorToken.TokenColumn, TokenCategory.Operator));
            }
        }

        /// <summary>
        /// Converts the operator expression to its string representation for code generation.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A string representation of the operator expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 0);
            return $"{indentLine}Operator: {base.ToCode(ident)}";
        }
    }
}
