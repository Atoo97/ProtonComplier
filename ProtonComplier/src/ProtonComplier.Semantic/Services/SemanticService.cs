// <copyright file="SemanticService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic.Services
{
    using Proton.Parser.Statements;
    using Proton.Semantic.Interfaces;

    /// <summary>
    /// Provides a high-level orchestration service for semantic analysis,
    /// including validating parsed statements for logical consistency, symbol usage, and type correctness.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SemanticService"/> class with the specified semantic analyzer.
    /// </remarks>
    /// <param name="semanticAnalyzer">An instance of <see cref="ISemanticAnalyzer"/> responsible for analyzing parsed statement sections.</param>
    public class SemanticService(ISemanticAnalyzer semanticAnalyzer)
    {
        private readonly ISemanticAnalyzer semanticAnalyzer = semanticAnalyzer;

        /// <summary>
        /// Performs the complete semantic analysis process on the provided parsed statement sections.
        /// Validates the semantic correctness of the code, including symbol resolution and rule enforcement.
        /// </summary>
        /// <param name="sections">A dictionary containing parsed statements grouped by macro sections.</param>
        /// <returns>A <see cref="SemanticResult"/> representing the outcome of the semantic analysis, including any errors or warnings found.</returns>
        public SemanticResult Complie(Dictionary<string, List<Statement>> sections)
        {
            return this.semanticAnalyzer.Analyze(sections);
        }
    }

}
