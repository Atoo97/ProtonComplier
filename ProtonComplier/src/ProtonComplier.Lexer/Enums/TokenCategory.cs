// <copyright file="TokenCategory.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer.Enums
{
    /// <summary>
    /// Defines the main lexical categories for tokens in the Proton language.
    /// Used to classify tokens during lexical analysis.
    /// </summary>
    public enum TokenCategory
    {
        /// <summary>
        /// Reserved language keywords (e.g., N, $, B, ).
        /// </summary>
        Keyword,

        /// <summary>
        /// User-defined identifiers such as variable names, function names, etc.
        /// </summary>
        Identifier,

        /// <summary>
        /// Constant values like numbers, strings, booleans, and characters.
        /// </summary>
        Literal,

        /// <summary>
        /// Arithmetic, logical, and comparison operators (e.g., +, -, ==).
        /// </summary>
        Operator,

        /// <summary>
        /// Structural symbols used for grouping and separating (e.g., ; , ( ) { }).
        /// </summary>
        Punctuator,

        /// <summary>
        /// Special tokens not fitting into other categories (e.g., macro, comment).
        /// </summary>
        Special,
    }
}
