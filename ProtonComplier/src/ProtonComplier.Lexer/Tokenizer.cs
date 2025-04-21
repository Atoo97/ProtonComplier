// <copyright file="Tokenizer.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using Proton.Lexer.Interfaces;

    /// <summary>
    /// Responsible for tokenizing a string input based on predefined token definitions.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Tokenizer"/> class.
    /// </remarks>
    public class Tokenizer : ITokenizer
    {
        private static readonly char[] Separator = new[] { ' ', '\t' };
        private readonly List<TokenDefinition> definitions = RegexGrammar.GetTokenDefinitions();
        private int line = 1;
        private int column = 1;

        /// <summary>
        /// Processes the input string and converts it into a list of <see cref="Token"/> objects.
        /// Identifies known lexical patterns and reports any unrecognized tokens as errors.
        /// </summary>
        /// <param name="input">The raw source code or text to tokenize.</param>
        /// <returns>A list of <see cref="Token"/> objects representing the lexical structure of the input.</returns>
        public List<Token> Tokenize(string input)
        {
            // Split input into lines
            var lines = input.Split('\n');
            var tokens = new List<Token>();

            foreach (var line in lines)
            {
                var lineTokens = this.TokenizeLine(line);
                tokens.AddRange(lineTokens);
                this.line++;
            }

            return tokens;
        }

        /// <summary>
        /// Tokenizes a single line of input while keeping track of line and column numbers.
        /// </summary>
        /// <param name="line">The line of input to tokenize.</param>
        /// <returns>A list of <see cref="Token"/> representing the tokens found in the line.</returns>
        private List<Token> TokenizeLine(string line)
        {
            var lineTokens = new List<Token>();
            int linePosition = 0;                           // Track position within the current line

            while (linePosition < line.Length)
            {
                bool matchFound = false;

                foreach (var def in this.definitions)
                {
                    var match = def.TokenPattern.Match(line, linePosition);
                    if (match.Success)
                    {
                        string value = match.Groups[1].Value;
                        int matchLength = match.Length;
                        int startColumn = this.column + linePosition;

                        // Handle comments: Remove "//"
                        if (def.TokenName == TokenType.Comment)
                        {
                            value = value[2..].Trim('\r', '\n');
                            matchLength = value.Length + 2;
                        }

                        // Handle macros: Remove "#" only from the start
                        if (def.TokenName == TokenType.Macro)
                        {
                            value = value.TrimStart('#'); // Removes only the leading '#'

                            // Find the actual macro name (first word)
                            string[] parts = value.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length > 0)
                            {
                                string macroName = parts[0]; // The macro itself
                                string remainingText = parts.Length > 1 ? parts[1] : string.Empty; // The part after the macro

                                // If remainingText is empty and not contains non-whitespace characters, it's a valid macro format
                                if (string.IsNullOrWhiteSpace(remainingText))
                                {
                                    value = macroName; // Keep only the macro name
                                }
                            }

                            matchLength = value.Length + 1;
                        }

                        // Handle Character literal: Remove surrounding single quotes
                        if (def.TokenName == TokenType.Character)
                        {
                            value = value.Trim('\'').Trim('\r', '\n');
                            matchLength = value.Length + 2; // Account for removed single quotes
                        }

                        // Handle Srting literal:  Remove surrounding double quotes
                        if (def.TokenName == TokenType.String)
                        {
                            value = value.Trim('"').Trim('\r', '\n');
                            matchLength = value.Length + 2;
                        }

                        // Create token with line and column data
                        lineTokens.Add(new Token
                        {
                            TokenType = def.TokenName,
                            TokenValue = value,
                            TokenLine = this.line,
                            TokenColumn = startColumn,
                        });

                        // Update linePosition
                        linePosition += matchLength;
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    // Capture the unknown token
                    char unknownChar = line[linePosition];

                    // Add an unknown token with error location
                    lineTokens.Add(new Token
                    {
                        TokenType = TokenType.Unknown,
                        TokenValue = unknownChar.ToString(),
                        TokenLine = this.line,
                        TokenColumn = this.column + linePosition,
                    });

                    // Move to the next character
                    linePosition++;
                }

                if (linePosition < line.Length && line[linePosition] == '\n')
                {
                    this.line++; // Update line number if needed (this is more relevant when processing multiple lines)
                    this.column = 1; // Reset column to 1 at the start of a new line
                }
            }

            // change last whitespace to newline
            if (lineTokens.Count > 0)
            {
                lineTokens[^1].TokenType = TokenType.Newline;
                lineTokens[^1].TokenValue = "\n";
            }

            return lineTokens;
        }
    }
}
