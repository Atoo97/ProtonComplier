// <copyright file="TokenDefinition.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using System.Text.RegularExpressions;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Defines a token's type and the regular expression pattern used to recognize it.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDefinition"/> class.
    /// </remarks>
    /// <param name="tokenName">The token's type (e.g., identifier, keyword, literal).</param>
    /// <param name="category">The high-level category the token belongs to (e.g., Keyword, Literal, Operator).</param>
    /// <param name="pattern">The regex pattern that defines the structure of the token.</param>
    public class TokenDefinition(TokenType tokenName, TokenCategory category, Regex pattern)
    {
        /// <summary>
        /// Gets the type of the token (e.g., identifier, keyword, literal, etc.)
        /// </summary>
        public TokenType TokenName { get; } = tokenName;

        /// <summary>
        /// Gets the lexical category of the token, grouping similar types for parsing and classification.
        /// </summary>
        public TokenCategory TokenCategory { get; } = category;

        /// <summary>
        /// Gets the regular expression pattern used to match the token in the input string.
        /// </summary>
        public Regex TokenPattern { get; } = pattern;
    }
}