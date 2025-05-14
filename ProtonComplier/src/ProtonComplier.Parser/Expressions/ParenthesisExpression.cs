// <copyright file="ParenthesisExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.Lexer;

    /// <summary>
    /// Represents an expression enclosed in parentheses, such as (2 + 3).
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ParenthesisExpression"/> class.
    /// Parses the inner expression from the provided list of tokens.
    /// </remarks>
    /// <param name="tokens">The list of tokens within the parentheses.</param>
    public class ParenthesisExpression(List<Token> tokens)
        : Expression(tokens.First())
    {
        /// <summary>
        /// Gets the inner expression contained within the parentheses.
        /// </summary>
        public Expression InnerExpression { get; } = ExpressionParserHelper.ParseExpression(tokens);

        /// <summary>
        /// Gets the last token of the inner expression inside the parentheses.
        /// </summary>
        public Token LastToken
        {
            get
            {
                return GetLastToken(this.InnerExpression);
            }
        }

        /// <summary>
        /// Converts the expression to a formatted code string for debugging or output purposes.
        /// </summary>
        /// <param name="ident">The indentation level for formatting.</param>
        /// <returns>A string representing the expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 8); // 4 spaces per indent level
            return $"ParenthesisExpression:\n" +
                   $"{indentLine}├─ {this.InnerExpression.ToCode(ident + 1)}";
        }

        /// <summary>
        /// Recursively retrieves the last token of the provided expression.
        /// This method traverses through different types of expressions to find the last token,
        /// such as in operand expressions, operator expressions, and binary or parenthetical expressions.
        /// </summary>
        /// <param name="expr">The expression whose last token is to be retrieved.</param>
        /// <returns>The last token of the provided expression.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the expression type is unsupported or cannot have a last token.
        /// </exception>
        private static Token GetLastToken(Expression expr)
        {
            return expr switch
            {
                OperandExpression operand => operand.ParseSymbol!,
                OperatorExpression op => op.ParseSymbol!,
                BinaryExpression binary => GetLastToken(binary.Right), // Last token is in the rightmost sub-expression
                ParenthesisExpression paren => paren.LastToken,
                ListNthElementExpression listNth => listNth.CloseSqrBrace,
                LengthExpression lnexpr => lnexpr.Lenght,
                MaxExpression max => GetLastToken(max.RightExpression),
                MinExpression min => GetLastToken(min.RightExpression),
                OptionalExpression opt => GetLastToken(opt.RightExpression),
                // VariableInitializationExpression varExpr => varExpr.RightNode,

                // Add other cases as needed
                _ => throw new InvalidOperationException("Unsupported expression type for retrieving last token."),
            };
        }
    }
}
