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
                // Chehck if listNthElement or Max/Min or .Length
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
                else if (tokens.Count == 3 && tokens[0].TokenType == TokenType.Identifier && tokens[1].TokenType == TokenType.Period)
                {
                    return new LengthExpression(new OperandExpression(tokens[0]), tokens[1], tokens[2]);
                }
                else if (tokens.Count > 3 && tokens[0].TokenType == TokenType.Identifier && tokens[1].TokenType == TokenType.Period)
                {
                    LengthExpression lengthExpression = new (new OperandExpression(tokens[0]), tokens[1], tokens[2]);
                    OperatorExpression op = new (tokens[3]);
                    List<Token> remaining = tokens.Skip(4).ToList();
                    return new BinaryExpression(lengthExpression, op, remaining);
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
                else if (tokens[0].TokenType == TokenType.Min)
                {
                    // Parse Min Expression (min(a, b))
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

                    // Create the MinExpression
                    var minExpr = new MinExpression(leftExpr, rightExpr);

                    // Now check if there are additional tokens (operator + operand for BinaryExpression)
                    List<Token> remainingTokens = tokens.Skip(i + 1).ToList();

                    if (remainingTokens.Count > 0)
                    {
                        OperatorExpression op = new (remainingTokens[0]);  // Assuming first token is operator
                        remainingTokens = remainingTokens.Skip(1).ToList();  // Skip the operator
                        return new BinaryExpression(minExpr, op, remainingTokens);
                    }

                    // If no remaining tokens, return the MinExpression itself
                    return minExpr;
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
