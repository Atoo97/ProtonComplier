// <copyright file="ParserResult.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser
{
    using Proton.ErrorHandler;
    using Proton.Parser.Statements;

    /// <summary>
    /// Represents the result of parser analysis, including tokens, errors, and macro sections.
    /// </summary>
    public readonly record struct ParserResult(
        List<BaseException> errors,
        List<BaseException> warnings,
        Dictionary<string, List<Statement>> sections,
        bool isSuccessful);
}
