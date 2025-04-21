// <copyright file="LexicalResult.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using Proton.ErrorHandler;

    /// <summary>
    /// Represents the result of lexical analysis, including tokens, errors, and macro sections.
    /// </summary>
    public readonly record struct LexicalResult(
        List<BaseException> errors,
        List<BaseException> warnings,
        Dictionary<string, List<Token>> sections,
        bool isSuccessful);
}