// <copyright file="ParserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Service
{
    using Proton.Lexer;
    using Proton.Parser.Interfaces;

    /// <summary>
    /// Provides a high-level orchestration service for syntax analysis,
    /// including parsing tokenized input and validating its structure.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ParserService"/> class with the specified syntax analyzer.
    /// </remarks>
    /// <param name="analyzer">An instance of <see cref="ISyntaxAnalyzer"/> responsible for analyzing the tokenized sections.</param>
    public class ParserService(ISyntaxAnalyzer analyzer)
    {
        private readonly ISyntaxAnalyzer analyzer = analyzer;

        /// <summary>
        /// Performs the complete syntax analysis process on the provided tokenized sections.
        /// Analyzes the input sections for proper syntax, structural errors, and overall validity.
        /// </summary>
        /// <param name="sections">A dictionary containing tokenized input grouped by sections.</param>
        /// <returns>A <see cref="ParserResult"/> containing the result of the analysis, including any errors or warnings detected during parsing.</returns>
        public ParserResult Complie(Dictionary<string, List<Token>> sections)
        {
            return this.analyzer.Analyze(sections);
        }
    }
}