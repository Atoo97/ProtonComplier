// <copyright file="Token.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using Proton.Lexer.Enums;

    /// <summary>
    /// Token class holding: Type, Value, Metadata.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets or Sets the type of the token (e.g., identifier, keyword, literal, etc.)
        /// </summary>
        public TokenType TokenType { get; set; }

        /// <summary>
        /// Gets or Sets the lexical category of the token, grouping similar types for parsing and classification.
        /// </summary>
        public TokenCategory TokenCategory { get; set; }

        /// <summary>
        /// Gets or Sets the value of the token (e.g., actual text of the token).
        /// </summary>
        public string TokenValue { get; set; } = null!;

        /// <summary>
        /// Gets or Sets the line number where the token was found in the source code.
        /// </summary>
        public int TokenLine { get; set; }

        /// <summary>
        /// Gets or Sets the column number where the token was found in the source code.
        /// </summary>
        public int TokenColumn { get; set; }

        /// <summary>
        /// Override of ToString to provide a basic string representation of the token.
        /// </summary>
        /// <returns>Formatted string with token type and value.</returns>
        public override string ToString() => $"[Tokentype: {this.TokenType}] - [TokenValue: {this.TokenValue}]";

        /// <summary>
        /// Another version of displaying token information, with metadata details (line and column).
        /// If detailed is true, it returns a more descriptive string.
        /// </summary>
        /// <param name="detailed">Whether to return a detailed string with metadata or just a basic string.</param>
        /// <returns>Formatted string with token details.</returns>
        public string DisplayToken(bool detailed)
        {
            if (detailed)
            {
                return $"[Tokentype: {this.TokenType}] - [TokenValue: {this.TokenValue}] - [Line: {this.TokenLine}, Column: {this.TokenColumn}]";
            }

            return this.ToString();  // or Call the default version
        }
    }
}
