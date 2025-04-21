// <copyright file="LexicalService.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer.Services
{
    using Proton.Lexer.Interfaces;

    /// <summary>
    /// Provides a high-level orchestration service for running lexical analysis,
    /// including tokenization and structural validation of input.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LexicalService"/> class with the specified tokenizer and analyzer.
    /// </remarks>
    /// <param name="tokenizer">An instance of <see cref="ITokenizer"/> responsible for breaking input into tokens.</param>
    /// <param name="analyzer">An instance of <see cref="ILexicalAnalyzer"/> responsible for analyzing token structure and validity.</param>
    public class LexicalService(ITokenizer tokenizer, ILexicalAnalyzer analyzer)
    {
        private readonly ITokenizer tokenizer = tokenizer;
        private readonly ILexicalAnalyzer analyzer = analyzer;

        /// <summary>
        /// Performs the complete lexical analysis process on the provided source code.
        /// First tokenizes the input, then analyzes the tokens for structure, macro sections,
        /// and any lexical errors or warnings.
        /// </summary>
        /// <param name="code">The source code to be tokenized and analyzed.</param>
        /// <returns>A <see cref="LexicalResult"/> containing tokens grouped by macro sections, along with any detected errors, warnings, and overall success status.</returns>
        public LexicalResult Complie(string code)
        {
            var tokens = this.tokenizer.Tokenize(code);
            return this.analyzer.Analyze(tokens);
        }
    }
}