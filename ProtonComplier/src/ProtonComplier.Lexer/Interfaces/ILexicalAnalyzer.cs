// <copyright file="ILexicalAnalyzer.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer.Interfaces
{
    /// <summary>
    /// Defines the contract for a lexical analyzer responsible for validating token sequences,
    /// identifying macro sections, and collecting lexical diagnostics such as errors and warnings.
    /// </summary>
    /// <remarks>
    /// This interface is typically used after tokenization to ensure the lexical structure of the code
    /// adheres to expected patterns, and to provide meaningful feedback for developers.
    /// </remarks>
    public interface ILexicalAnalyzer
    {
        /// <summary>
        /// Performs lexical analysis on the provided list of tokens, identifying macro sections,
        /// validating structure, and collecting any lexical errors or warnings encountered during analysis.
        /// </summary>
        /// <param name="tokens">The list of tokens to analyze.</param>
        /// <returns>
        /// A <see cref="LexicalResult"/> containing:
        /// <list type="bullet">
        ///   <item><description>Detected lexical errors and warnings</description></item>
        ///   <item><description>Macro section groupings with their associated tokens</description></item>
        ///   <item><description>A flag indicating whether the analysis passed without critical errors</description></item>
        /// </list>
        /// </returns>
        LexicalResult Analyze(List<Token> tokens);
    }
}
