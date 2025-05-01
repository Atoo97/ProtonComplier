// <copyright file="ISemanticAnalyzer.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic.Interfaces
{
    using Proton.Parser.Statements;

    /// <summary>
    /// Defines the contract for a semantic analyzer responsible for validating parsed statement sections,
    /// ensuring logical correctness, symbol resolution, and proper usage of declared entities.
    /// </summary>
    /// <remarks>
    /// This interface is typically used after syntax analysis. It performs deeper validation of the program's meaning,
    /// such as type checking, symbol table verification, and semantic rule enforcement.
    /// </remarks>
    public interface ISemanticAnalyzer
    {
        /// <summary>
        /// Analyzes the provided parsed sections for semantic correctness, ensuring consistent use of variables,
        /// types, and other logical constructs in accordance with language rules.
        /// </summary>
        /// <param name="sections">A dictionary containing parsed statements grouped by macro sections.</param>
        /// <returns>
        /// A <see cref="SemanticResult"/> containing:
        /// <list type="bullet">
        ///   <item><description>Semantic errors and warnings detected during analysis</description></item>
        ///   <item><description>Overall status indicating whether the analyzed code is semantically valid</description></item>
        /// </list>
        /// </returns>
        SemanticResult Analyze(Dictionary<string, List<Statement>> sections);
    }
}
