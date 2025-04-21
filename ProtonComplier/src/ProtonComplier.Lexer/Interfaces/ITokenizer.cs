// <copyright file="ITokenizer.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer.Interfaces
{
    /// <summary>
    /// Defines the contract for a tokenizer that converts an input string into a sequence of tokens
    /// based on a set of predefined lexical rules.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface should handle parsing logic, error detection,
    /// and token classification for use in lexical analysis.
    /// </remarks>
    public interface ITokenizer
    {
        /// <summary>
        /// Processes the input string and converts it into a list of <see cref="Token"/> objects.
        /// Identifies known lexical patterns and reports any unrecognized tokens as errors.
        /// </summary>
        /// <param name="input">The raw source code or text to tokenize.</param>
        /// <returns>A list of <see cref="Token"/> objects representing the lexical structure of the input.</returns>
        List<Token> Tokenize(string input);
    }
}
