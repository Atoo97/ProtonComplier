// <copyright file="ExpressionParserHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using System.Collections.Generic;
    using System.Linq;
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Provides helper methods for parsing expressions in the Proton parser.
    /// </summary>
    public static class ExpressionParserHelper
    {
        /// <summary>
        /// Parses a list of tokens into an <see cref="Expression"/> object.
        /// Supports parenthesis grouping and binary expressions.
        /// </summary>
        /// <param name="tokens">The list of tokens to parse.</param>
        /// <returns>An <see cref="Expression"/> representing the parsed expression tree.</returns>
        /// <exception cref="AnalyzerError">
        /// Thrown when parentheses are empty or unmatched, or if tokens are invalid for an expression.
        /// </exception>
        public static Expression ParseExpression(List<Token> tokens)
        {
            if (tokens[0].TokenType == TokenType.OpenParen)
            {
                int closeIndex = FindMatchingParen(tokens, 0);
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

                return left;
            }
            else if (tokens.Count == 1)
            {
                return new OperandExpression(tokens[0]);
            }
            else
            {
                OperandExpression left = new (tokens[0]);
                OperatorExpression op = new (tokens[1]);
                List<Token> remaining = tokens.Skip(2).ToList();
                return new BinaryExpression(left, op, remaining);
            }
        }

        /// <summary>
        /// Finds the index of the matching closing parenthesis for the opening parenthesis at the specified index.
        /// </summary>
        /// <param name="tokens">The list of tokens.</param>
        /// <param name="startIndex">The index of the opening parenthesis.</param>
        /// <returns>The index of the matching closing parenthesis.</returns>
        /// <exception cref="AnalyzerError">Thrown when no matching closing parenthesis is found.</exception>
        public static int FindMatchingParen(List<Token> tokens, int startIndex)
        {
            int openCount = 0;
            for (int i = startIndex; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.OpenParen)
                {
                    openCount++;
                }
                else if (tokens[i].TokenType == TokenType.CloseParen)
                {
                    openCount--;
                }

                if (openCount == 0)
                {
                    return i;
                }
            }

            // If no matching close paren was found
            var lastToken = tokens.LastOrDefault();
            int errorLine = lastToken?.TokenLine ?? 0;
            int errorColumn = lastToken?.TokenColumn ?? 0;

            // Generate error message
            throw new AnalyzerError(
                "121",
                string.Format(MessageRegistry.GetMessage(121).Text, errorLine, errorColumn));
        }
    }
}
