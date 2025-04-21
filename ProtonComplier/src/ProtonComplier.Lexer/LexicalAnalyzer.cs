// <copyright file="LexicalAnalyzer.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using System.Collections.Generic;
    using Proton.ErrorHandler;
    using Proton.Lexer.Interfaces;
    using Proton.Lexer.Macro;

    /// <summary>
    /// Provides functionality to analyze a sequence of tokens and identify macros, validate structure,
    /// and collect lexical errors or warnings for visual feedback.
    /// </summary>
    public class LexicalAnalyzer : ILexicalAnalyzer
    {
        // Stores macro definitions
        private static readonly Dictionary<string, List<string>> Definitions = new ();

        // List to store errors and warnings
        private static readonly List<BaseException> Errors = new ();
        private static readonly List<BaseException> Warnings = new ();

        // Stores tokens grouped by macro sections
        private static readonly Dictionary<string, List<Token>> Sections = new ();

        private static readonly List<Token> CurrentContent = new ();
        private static string? currentMacro;

        /// <summary>
        /// Before starting a new lexical analysis, reset everything.
        /// </summary>
        public static void Reset()
        {
            Definitions.Clear();
            Sections.Clear();
            CurrentContent.Clear();
            currentMacro = null;
            Errors.Clear();
            Warnings.Clear();
        }

        /// <summary>
        /// Analyzes a list of tokens for lexical correctness. It groups tokens into macro sections,
        /// validates the structure and placement of tokens, checks for missing or duplicate macro definitions,
        /// and collects any errors or warnings encountered during analysis.
        /// </summary>
        /// <param name="tokens">The list of tokens to be analyzed.</param>
        /// <returns>
        /// A <see cref="LexicalResult"/> containing:
        /// <list type="bullet">
        ///   <item><description><see cref="LexicalResult.errors"/> – A list of lexical errors encountered</description></item>
        ///   <item><description><see cref="LexicalResult.warnings"/> – A list of non-critical lexical warnings</description></item>
        ///   <item><description><see cref="LexicalResult.sections"/> – A dictionary mapping macro section names to their associated tokens</description></item>
        ///   <item><description><see cref="LexicalResult.isSuccessful"/> – A boolean indicating whether the analysis passed without errors</description></item>
        /// </list>
        /// </returns>
        public LexicalResult Analyze(List<Token> tokens)
        {
            Reset();

            if (tokens.Count == 0 || tokens.All(t => string.IsNullOrWhiteSpace(t.TokenValue)))
            {
                // Generate error message
                Errors.Add(new AnalyzerError(
                    "001",
                    string.Format(MessageRegistry.GetMessage(001).Text)));

                return new LexicalResult(Errors, Warnings, null!, false);
            }

            foreach (Token token in tokens)
            {
                // Check if token is a macro section (starting with #)
                if (token.TokenType == TokenType.Macro)
                {
                    // Validate if the macro value is valid (ERROR if not exist this name)
                    if (!MacroType.ExpectedMacros.Values.Contains(token.TokenValue))
                    {
                        Errors.Add(new AnalyzerError(
                             "004",
                             string.Format(MessageRegistry.GetMessage(004).Text, token.TokenValue, token.TokenLine, token.TokenColumn, string.Join(", ", MacroType.ExpectedMacros.Values))));
                    }

                    // If exist macro def name
                    else // Handle macro definitions
                    {
                        string key = token.TokenValue.Split(' ')[0]; // Get the macro name

                        // Ensure we always have a key for the macro in the definitions dictionary
                        if (!Definitions.ContainsKey(key))
                        {
                            Definitions[key] = new ();
                        }

                        // Extract the value after the macro prefix (e.g., "N:123", "R:123") and add it to the list
                        string value = token.TokenValue[key.Length..].Trim();

                        // Add the new definition under the macro key
                        Definitions[key].Add(value);
                    }

                    // Save the previous macro section
                    if (currentMacro != null)
                    {
                        // Add to the sections dictionary. If the macro already exists, append to the existing list.
                        if (Sections.TryGetValue(currentMacro, out List<Token>? value))
                        {
                            value.AddRange(CurrentContent);
                        }
                        else
                        {
                            Sections[currentMacro] = new List<Token>(CurrentContent);
                        }

                        CurrentContent.Clear();
                    }

                    // Set the new macro section
                    currentMacro = token.TokenValue;
                }

                // ERROR for value declaration without macro def!!
                else if (currentMacro == null && token.TokenType != TokenType.Comment && token.TokenType != TokenType.Whitespace && token.TokenType != TokenType.Newline)
                {
                    // Validate token format and check for misplaced values
                    // Validate token format and appears new error because before any macro section.
                    if (currentMacro == null)
                    {
                        // For error handling (Invalid value placement)
                        Errors.Add(new AnalyzerError(
                             "008",
                             string.Format(MessageRegistry.GetMessage(008).Text, token.TokenValue, token.TokenLine, token.TokenColumn)));
                    }
                }

                // ERROR: for unknown token
                else if (currentMacro != null && token.TokenType == TokenType.Unknown)
                {
                    // handle unknown token errors:
                    Errors.Add(new AnalyzerError(
                           "011",
                           string.Format(MessageRegistry.GetMessage(011).Text, token.TokenValue, token.TokenLine, token.TokenColumn)));
                }
                else if (currentMacro != null)
                {
                    // Add tokens to the current section
                    CurrentContent.Add(token);
                }
            }

            // Store the last section (if applicable)
            if (currentMacro != null && CurrentContent.Count > 0)
            {
                // Add to the sections dictionary. If the macro already exists, append to the existing list.
                if (Sections.TryGetValue(currentMacro, out List<Token>? value))
                {
                    value.AddRange(CurrentContent);
                }
                else
                {
                    Sections[currentMacro] = new List<Token>(CurrentContent);
                }
            }

            // --- VALIDATION: Check for missing sections ---
            List<string> missingMacros = MacroType.ExpectedMacros
                .Where(m => !Sections.ContainsKey(m.Value)) // m.Key is MacroType
                .Select(m => m.Value) // Convert to string (get dictionary value)
                .ToList();

            // Error handling:
            if (missingMacros.Count > 0)
            {
                foreach (string missing in missingMacros)
                {
                    // Add the missing section error
                    Errors.Add(new AnalyzerError(
                         "005",
                         string.Format(MessageRegistry.GetMessage(005).Text, missing)));  // Use null for line/column if not available
                }
            }

            // --- WARNING: Check for multiple macros
            foreach (var macro in Definitions)
            {
                if (macro.Value.Count > 1)
                {
                    // Find the first token associated with this macro
                    Token? firstToken = Sections.ContainsKey(macro.Key) && Sections[macro.Key].Count > 0
                        ? Sections[macro.Key].First()
                        : null;

                    int line = firstToken?.TokenLine ?? -1;
                    int column = firstToken?.TokenColumn ?? -1;

                    Warnings.Add(new AnalyzerWarning(
                        "006",
                        string.Format(MessageRegistry.GetMessage(006).Text, macro.Key, line, column)));
                }
            }

            // Return results
            bool isSuccessful = Errors.Count == 0;

            return new LexicalResult(Errors, Warnings, Sections, isSuccessful);
        }
    }
}
