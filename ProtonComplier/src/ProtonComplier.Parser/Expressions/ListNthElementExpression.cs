// <copyright file="ListNthElementExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents an expression accessing the N-th element of a list or array by index.
    /// This class handles constructs like <c>list[2]</c> or <c>array[-1]</c>, where the
    /// index is provided as an operand expression within square brackets.
    /// </summary>
    public class ListNthElementExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListNthElementExpression"/> class.
        /// Validates that the provided tokens form a valid list index access expression, such as <c>list[0]</c>.
        /// </summary>
        /// <param name="identifier">The identifier token representing the list or array name.</param>
        /// <param name="openSqrBrace">The token representing the opening square bracket.</param>
        /// <param name="op">The operand expression representing the index value.</param>
        /// <param name="closeSqrBrace">The token representing the closing square bracket.</param>
        /// <exception cref="AnalyzerError">Thrown when the tokens do not form a valid list index access.</exception>
        public ListNthElementExpression(OperandExpression identifier, Token openSqrBrace, OperandExpression op, Token closeSqrBrace)
          : base(identifier.ParseSymbol)
        {
            if (identifier.ParseSymbol.TokenType != TokenType.Identifier && openSqrBrace.TokenType != TokenType.OpenSqrBrace && closeSqrBrace.TokenType != TokenType.CloseSqrBrace)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, identifier.ParseSymbol.TokenValue, identifier.ParseSymbol.TokenLine, identifier.ParseSymbol.TokenColumn, "List N'th element"));
            }

            if (op.ParseSymbol.TokenType != TokenType.Identifier && op.ParseSymbol.TokenType != TokenType.Uint && op.ParseSymbol.TokenType != TokenType.Int)
            {
                // Generate error message
                throw new AnalyzerError(
                    "105",
                    string.Format(MessageRegistry.GetMessage(105).Text, op.ParseSymbol.TokenValue, op.ParseSymbol.TokenLine, op.ParseSymbol.TokenColumn));
            }

            this.Identifier = identifier;
            this.OpenSqrBrace = openSqrBrace;
            this.Operand = op;
            this.CloseSqrBrace = closeSqrBrace;
        }

        /// <summary>
        /// Gets or sets the operand expression that represents the identifier.
        /// </summary>
        public OperandExpression Identifier { get; set; }

        /// <summary>
        /// Gets or sets the token that represents the open square brackets.
        /// </summary>
        public Token OpenSqrBrace { get; set; }

        /// <summary>
        /// Gets or sets the operand expression that represents the index inside the square brackets.
        /// </summary>
        public OperandExpression Operand { get; set; }

        /// <summary>
        /// Gets or sets the token that represents the close square brackets.
        /// </summary>
        public Token CloseSqrBrace { get; set; }

        /// <summary>
        /// Converts the list index expression into its string representation for debugging or visualization.
        /// </summary>
        /// <param name="ident">The indentation level used to format the output.</param>
        /// <returns>A string representing the list element access expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 0);
            return $"{indentLine}ListNthElement: {base.ToCode(ident)}[{this.Operand.ToCode(1)}]";
        }
    }
}
