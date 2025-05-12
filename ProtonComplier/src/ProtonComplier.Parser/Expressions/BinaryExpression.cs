// <copyright file="BinaryExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a binary expression in the parse tree, such as "x + 3" or "1 + 3 * (1 - 2)".
    /// </summary>
    public class BinaryExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExpression"/> class.
        /// </summary>
        /// <param name="left">The left expression.</param>
        /// <param name="op">The operator token (e.g., '+', '-', '*').</param>
        /// <param name="remainingTokens">The remaining tokens for the right-hand side of the expression.</param>
        /// <exception cref="AnalyzerError">Thrown when the right-hand side is missing or improperly formed.</exception>
        public BinaryExpression(Expression left, OperatorExpression op, List<Token> remainingTokens)
            : base(left.ParseSymbol)
        {
            this.Left = left;
            this.Operator = op;

            if (remainingTokens == null || remainingTokens.Count == 0)
            {
                // Generate error message
                throw new AnalyzerError(
                    "124",
                    string.Format(MessageRegistry.GetMessage(124).Text, op.ParseSymbol.TokenLine + 1, op.ParseSymbol.TokenColumn + 1));
            }

            this.Right = ParseRightExpression(remainingTokens);
        }

        /// <summary>
        /// Gets the left-hand side expression of the binary expression.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// Gets the operator in the binary expression (e.g., '+', '-', '*').
        /// </summary>
        public OperatorExpression Operator { get; }

        /// <summary>
        /// Gets the right-hand side expression of the binary expression.
        /// </summary>
        public Expression Right { get; private set; }

        /// <summary>
        /// Generates a human-readable string representation of the binary expression, formatted with indentation.
        /// </summary>
        /// <param name="ident">The indentation level.</param>
        /// <returns>A formatted string representation of the binary expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4); // 4 spaces per indent level

            return $"BinaryExpression:\n" +
                   $"{indentLine}├─ LeftOperand: {this.Left.ToCode(ident + 1)}\n" +
                   $"{indentLine}├─ Operator: {this.Operator.ToCode(ident + 1)}\n" +
                   $"{indentLine}└─ RightOperand: {this.Right.ToCode(ident + 1)}";
        }

        /// <summary>
        /// Sets the right-hand side expression explicitly.
        /// </summary>
        /// <param name="right">The right-hand side expression to assign.</param>
        public void SetRight(Expression right)
        {
            this.Right = right;
        }

        /// <summary>
        /// Parses the right-hand side of the binary expression, handling parentheses and nested expressions.
        /// </summary>
        /// <param name="tokens">The tokens that represent the right-hand side expression.</param>
        /// <returns>An <see cref="Expression"/> object representing the parsed right-hand side.</returns>
        /// <exception cref="AnalyzerError">Thrown if the parentheses are empty or malformed.</exception>
        private static Expression ParseRightExpression(List<Token> tokens)
        {
            if (tokens[0].TokenType == TokenType.OpenParen)
            {
                int closeIndex = ExpressionParserHelper.FindMatchingParen(tokens, 0);
                var inner = tokens.Skip(1).Take(closeIndex - 1).ToList();

                // Check for empty parentheses
                if (inner.Count == 0)
                {
                    var open = tokens[0];

                    // Generate error message
                    throw new AnalyzerError(
                        "127",
                        string.Format(MessageRegistry.GetMessage(127).Text, open.TokenLine + 1, open.TokenColumn + 1));
                }

                Expression left = new ParenthesisExpression(inner);

                if (closeIndex + 1 < tokens.Count)
                {
                    OperatorExpression op = new (tokens[closeIndex + 1]);
                    List<Token> remaining = tokens.Skip(closeIndex + 2).ToList();
                    return new BinaryExpression(left, op, remaining);
                }

                return left; // Parenthesized expression only
            }
            else if (tokens.Count == 1)
            {
                return new OperandExpression(tokens[0]);
            }
            else if (tokens[0].TokenType == TokenType.Max)
            {
                // Parse Max Expression (max(a, b))
                if (tokens[1].TokenType != TokenType.OpenParen)
                {
                    // Generate error message
                    throw new AnalyzerError(
                        "120",
                        string.Format(MessageRegistry.GetMessage(120).Text, tokens[1].TokenLine, tokens[1].TokenColumn));
                }

                List<Token> leftTokens = new ();
                List<Token> rightTokens = new ();
                int i = 2;

                // Collect left side tokens until the comma
                while (i < tokens.Count && tokens[i].TokenType != TokenType.Comma)
                {
                    leftTokens.Add(tokens[i]);
                    i++;
                }

                // Check if we found the comma
                if (i >= tokens.Count || tokens[i].TokenType != TokenType.Comma)
                {
                    // Generate error message
                    throw new AnalyzerError(
                       "104",
                       string.Format(MessageRegistry.GetMessage(104).Text, tokens[i - 1].TokenType, tokens[i - 1].TokenLine, tokens[i - 1].TokenColumn, TokenType.Comma));
                }

                i++; // Skip the comma

                // Collect right side tokens after the comma
                while (i < tokens.Count && tokens[i].TokenType != TokenType.CloseParen)
                {
                    rightTokens.Add(tokens[i]);
                    i++;
                }

                // Check if closing paren exists
                if (i >= tokens.Count || tokens[i].TokenType != TokenType.CloseParen)
                {
                    // Generate error message
                    throw new AnalyzerError(
                        "121",
                        string.Format(MessageRegistry.GetMessage(121).Text, tokens.Last().TokenLine, tokens.Last().TokenColumn));
                }

                // Parse both expressions
                var leftExpr = ExpressionParserHelper.ParseExpression(leftTokens);
                var rightExpr = ExpressionParserHelper.ParseExpression(rightTokens);

                // Create the MaxExpression
                var maxExpr = new MaxExpression(leftExpr, rightExpr);

                // Now check if there are additional tokens (operator + operand for BinaryExpression)
                List<Token> remainingTokens = tokens.Skip(i + 1).ToList();

                if (remainingTokens.Count > 0)
                {
                    OperatorExpression op = new (remainingTokens[0]);  // Assuming first token is operator
                    remainingTokens = remainingTokens.Skip(1).ToList();  // Skip the operator
                    return new BinaryExpression(maxExpr, op, remainingTokens);
                }

                // If no remaining tokens, return the MaxExpression itself
                return maxExpr;
            }
            else
            {
                // Chehck if listNthElement
                if (tokens.Count == 4 && tokens[1].TokenType == TokenType.OpenSqrBrace)
                {
                    return new ListNthElementExpression(new OperandExpression(tokens[0]), tokens[1], new OperandExpression(tokens[2]), tokens[3]);
                }
                else if (tokens.Count > 4 && tokens[1].TokenType == TokenType.OpenSqrBrace)
                {
                    ListNthElementExpression listNthElementExpression = new (new OperandExpression(tokens[0]), tokens[1], new OperandExpression(tokens[2]), tokens[3]);
                    OperatorExpression op = new (tokens[4]);
                    List<Token> remaining = tokens.Skip(5).ToList();
                    return new BinaryExpression(listNthElementExpression, op, remaining);
                }
                else
                {
                    OperandExpression left = new (tokens[0]);
                    OperatorExpression op = new (tokens[1]);
                    List<Token> remaining = tokens.Skip(2).ToList();
                    return new BinaryExpression(left, op, remaining);
                }
            }
        }
    }
}
