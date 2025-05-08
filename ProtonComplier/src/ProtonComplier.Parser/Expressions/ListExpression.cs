// <copyright file="ListExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace ProtonComplier.Parser.Expressions
{
    using System.Collections.Generic;
    using System.Linq;
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;
    using Proton.Parser.Expressions;

    /// <summary>
    /// Represents a list expression node that parses and stores multiple comma-separated expressions
    /// within braces (e.g., {expr1, expr2, ...}).
    /// </summary>
    public class ListExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListExpression"/> class by parsing tokens enclosed in braces.
        /// Supports nested expressions and warns on invalid comma usage.
        /// </summary>
        /// <param name="tokens">The token list including outer braces and inner expressions.</param>
        /// <param name="warnings">A collection to which parser warnings are added.</param>
        /// <exception cref="AnalyzerError">Thrown when closing brace is missing or when an empty element is encountered.</exception>
        public ListExpression(List<Token> tokens, List<BaseException> warnings)
            : base(tokens.First()) // '{'
        {
            // Empty list: {}
            if (tokens.Count == 1 && tokens[0].TokenType == TokenType.ValueSpecifier)
            {
                return;
            }

            this.Elements = [];

            if (tokens.Last().TokenType != TokenType.CloseBrace)
            {
                // Generate error message
                throw new AnalyzerError(
                    "123",
                    string.Format(MessageRegistry.GetMessage(123).Text, tokens.Last().TokenLine + 1, tokens.Last().TokenColumn + 1));
            }

            var innerTokens = tokens.Skip(1).Take(tokens.Count - 2).ToList();
            var splitLists = SplitByComma(innerTokens, warnings);

            foreach (var exprTokens in splitLists)
            {
                if (exprTokens.Count == 0)
                {
                    // Generate error message
                    throw new AnalyzerError(
                        "131",
                        string.Format(MessageRegistry.GetMessage(131).Text, this.ParseSymbol.TokenLine, this.ParseSymbol.TokenColumn));
                }

                var expr = ExpressionParserHelper.ParseExpression(exprTokens);
                this.Elements.Add(expr);
            }
        }

        /// <summary>
        /// Gets the list of parsed expression elements within the list expression.
        /// </summary>
        public List<Expression> Elements { get; } = null!;

        /// <summary>
        /// Returns a formatted string representation of the list expression, showing each element.
        /// </summary>
        /// <param name="ident">Indentation level (for pretty-printing nested structures).</param>
        /// <returns>A string representing the formatted structure of the list expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4); // 4 spaces per indent level

            // Handle the "ListExpression" heading
            string result = $"ListExpression:\n";

            if (this.Elements == null || this.Elements.Count == 0)
            {
                result += $"{indentLine}└─{base.ToCode(1)}";
            }
            else
            {
                // If there are elements, join them with commas and include indentation
                // Iterate through each element and join with commas
                var lines = this.Elements.Select(e => e.ToCode(ident + 1));
                result += $"{indentLine}└─{string.Join($",\n{indentLine}└─ ", lines)}";
            }

            return result;
        }

        private static List<List<Token>> SplitByComma(List<Token> tokens, List<BaseException> warnings)
        {
            var result = new List<List<Token>>();
            var current = new List<Token>();
            int depth = 0;
            bool lastTokenWasComma = false;

            foreach (var token in tokens)
            {
                // Track parentheses and braces depth to properly handle nested structures
                if (token.TokenType == TokenType.OpenParen || token.TokenType == TokenType.OpenBrace)
                {
                    depth++;
                }
                else if (token.TokenType == TokenType.CloseParen || token.TokenType == TokenType.CloseBrace)
                {
                    depth--;
                }

                // Detect consecutive commas at top level
                if (token.TokenType == TokenType.Comma && depth == 0)
                {
                    if (lastTokenWasComma)
                    {
                        // Issue warning for consecutive commas
                        warnings.Add(new AnalyzerWarning(
                                        "107",
                                        string.Format(MessageRegistry.GetMessage(107).Text, token.TokenLine, token.TokenColumn)));

                        continue; // Skip redundant comma
                    }
                    else
                    {
                        result.Add(current);
                        current = []; // Start a new list for the next part
                    }

                    lastTokenWasComma = true; // Mark that the last token was a comma
                }
                else
                {
                    current.Add(token); // Add the token to the current part
                    lastTokenWasComma = false; // Reset the comma flag
                }
            }

            // Add the last part (if any)
            if (current.Count > 0)
            {
                result.Add(current);
            }

            return result;
        }
    }
}
