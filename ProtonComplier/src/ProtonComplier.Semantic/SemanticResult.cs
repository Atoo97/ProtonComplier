// <copyright file="SemanticResult.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
    using Proton.ErrorHandler;
    using Proton.Parser.Statements;

    /// <summary>
    /// Represents the result of semantic analysis.
    /// </summary>
    public readonly record struct SemanticResult(
        List<BaseException> errors,
        List<BaseException> warnings,
        Dictionary<string, List<Statement>> sections,
        SymbolTable table,
        bool isSuccessful);
}
