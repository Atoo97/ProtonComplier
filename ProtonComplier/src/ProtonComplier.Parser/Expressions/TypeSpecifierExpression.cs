// <copyright file="TypeSpecifierExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a type specifier expression in the parse tree.
    /// A type specifier can be something like "N[]", "R", or "Z[]..".
    /// This class handles parsing and storing the list specifier and delimiters, as well as the type expression.
    /// </summary>
    public class TypeSpecifierExpression : TypeExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSpecifierExpression"/> class.
        /// Parses the given tokens to extract type specifiers, list indicators, and delimiters.
        /// </summary>
        /// <param name="tokens">The list of tokens to parse for the type specifier expression.</param>
        /// <exception cref="Exception">Thrown when a critical parsing error occurs due to malformed or unexpected tokens.</exception>
        public TypeSpecifierExpression(List<Token> tokens)
            : base(tokens[0], tokens.Any(t => t.TokenType == TokenType.ListSpecifier))
        {
            // Step 1: Parse remaining tokens and handle list specifiers and delimiters.
            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.ListSpecifier)
                {
                    // Ensure that only one list specifier exists.
                    if (this.ListSpecifier > 1)
                    {
                        // Generate error message
                        throw new AnalyzerError(
                            "112",
                            string.Format(MessageRegistry.GetMessage(112).Text, tokens[i].TokenLine, tokens[i].TokenColumn));
                    }

                    this.ListSpecifier++;  // Set ListSpecifier if token is []
                }
                else
                {
                    // Generate error message
                    throw new AnalyzerError(
                        "101",
                        string.Format(MessageRegistry.GetMessage(101).Text, tokens[i].TokenLine, tokens[i].TokenColumn, "TypeSpecifier ('[]' or ';')"));
                }
            }
        }

        /// <summary>
        /// Gets the list specifier for the type (e.g., "[]").
        /// This token represents the square brackets used for array or list type specifications.
        /// </summary>
        public int ListSpecifier { get; private set; } = 0;

        /// <summary>
        /// Generates the string representation of the <see cref="TypeSpecifierExpression"/> for code generation.
        /// Delegates to the base implementation to include the full structure of the type, including lists and delimiters,
        /// with appropriate indentation based on the provided level.
        /// </summary>
        /// <param name="ident">The indentation level.</param>
        /// <returns>A formatted string representing the type specifier expression.</returns>
        public override string ToCode(int ident)
        {
            return base.ToCode(ident);
        }
    }
}
