// <copyright file="OperandExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a single operand in the parse tree, such as a literal or an identifier.
    /// </summary>
    public class OperandExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperandExpression"/> class.
        /// Validates that the provided token is either a literal or an identifier.
        /// </summary>
        /// <param name="operandToken">The token representing the operand.</param>
        /// <exception cref="AnalyzerError">
        /// Thrown if the token is not a literal or identifier.
        /// </exception>
        public OperandExpression(Token operandToken)
          : base(operandToken)
        {
            if (operandToken.TokenCategory != TokenCategory.Literal && operandToken.TokenType != TokenType.Identifier)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, operandToken.TokenValue, operandToken.TokenLine, operandToken.TokenColumn, "Literal or identifier"));
            }
        }

        /// <summary>
        /// Generates the string representation of this operand for code generation.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A string representation of the operand expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 0);
            return $"{indentLine}Operand: {base.ToCode(ident)}";
        }
    }
}
