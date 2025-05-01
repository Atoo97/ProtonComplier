// <copyright file="ISyntaxAnalyzer.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Interfaces
{
    using Proton.Lexer;

    /// <summary>
    /// Defines the contract for a syntax analyzer responsible for validating tokenized sections,
    /// ensuring their proper syntactical structure, and collecting any syntax-related diagnostics.
    /// </summary>
    /// <remarks>
    /// This interface is typically used after lexical analysis to validate the syntax of the parsed code,
    /// ensuring that it adheres to the expected grammatical rules, and providing feedback for developers.
    /// </remarks>
    public interface ISyntaxAnalyzer
    {
        /// <summary>
        /// Analyzes the provided tokenized sections, checking for proper syntactical structure and
        /// identifying any syntax errors or warnings encountered during the analysis.
        /// </summary>
        /// <param name="sections">A dictionary containing tokenized input grouped by sections.</param>
        /// <returns>
        /// A <see cref="ParserResult"/> containing:
        /// <list type="bullet">
        ///   <item><description>Detected syntax errors and warnings</description></item>
        ///   <item><description>Validation result indicating whether the code is syntactically correct</description></item>
        /// </list>
        /// </returns>
        ParserResult Analyze(Dictionary<string, List<Token>> sections);
    }
}
