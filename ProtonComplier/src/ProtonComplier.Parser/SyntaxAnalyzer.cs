// <copyright file="SyntaxAnalyzer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser
{
    using System.Collections.Generic;
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;
    using Proton.Parser.Expressions;
    using Proton.Parser.Interfaces;
    using Proton.Parser.Statements;

    /// <summary>
    /// Represents the main parser class responsible for analyzing tokens from each macro section,
    /// generating syntax trees, and reporting errors and warnings.
    /// </summary>
    public class SyntaxAnalyzer : ISyntaxAnalyzer
    {
        // Stores statements grouped by macro sections
        private static readonly Dictionary<string, List<Statement>> Sections = new ();

        // List to store errors and warnings
        private static readonly List<BaseException> Errors = new ();
        private static readonly List<BaseException> Warnings = new ();

        private static readonly List<Token> Tokens = new ();
        private static int position;

        /// <summary>
        /// Gets the current token at the parsing position, or EndOfInput if out of bounds.
        /// </summary>
        private static Token CurrentToken => position < Tokens.Count ? Tokens[position] : new Token { TokenType = TokenType.EndOfInput };

        /// <summary>
        /// Before starting a new syntax analysis, reset everything.
        /// </summary>
        public static void Reset()
        {
            Sections.Clear();
            Tokens.Clear();
            position = 0;
            Errors.Clear();
            Warnings.Clear();
        }

        /// <summary>
        /// Parses the StateSpace macro section, handling variable declarations.
        /// </summary>
        public static void StatePlaceParser()
        {
            // Split tokens by new lines (each row is a list of tokens)
            List<List<Token>> splitedTokens = SplitTokensByNewline(Tokens);

            // Create a list to store variable declarations for this macro section
            List<Statement> variableDeclarations = new ();

            for (int i = 0; i < splitedTokens.Count; i++)
            {
                Tokens.Clear();
                Tokens.AddRange(splitedTokens[i]);  // it makes work the Eat/Peak() methods

                if (Tokens.Count == 0)
                {
                    break;
                }

                try
                {
                    position = 0; // Reset token position for each row
                    List<IdentifierExpression> variables = new ();

                    do
                    {
                        // 1) Step: Create identifier expression + check next expression exist
                        variables.Add(new IdentifierExpression(CurrentToken));
                        Eat(CurrentToken, "Separator");

                        bool warned = false;       // Track if we've already warned about consecutive commas
                        bool lastWasComma = false; // Tracks if the last token was a comma

                        // 2) Step: Check multiple identifier declaration order
                        while (CurrentToken.TokenType != TokenType.EndOfInput)
                        {
                            if (CurrentToken.TokenType == TokenType.Identifier)
                            {
                                variables.Add(new IdentifierExpression(CurrentToken));
                                Eat(CurrentToken, "Separator");

                                warned = false;         // Reset warning since we found a valid variable
                                lastWasComma = false;   // Reset comma tracker
                            }
                            else if (CurrentToken.TokenType == TokenType.Comma)
                            {
                                if (lastWasComma) // Detect multiple consecutive commas (var1,,,var2)
                                {
                                    if (!warned)
                                    {
                                        Warnings.Add(new AnalyzerWarning(
                                            "107",
                                            string.Format(MessageRegistry.GetMessage(107).Text, CurrentToken.TokenLine, CurrentToken.TokenColumn - 1)));

                                        warned = true; // Prevent duplicate warnings
                                    }
                                }

                                Eat(CurrentToken, "Identifier");
                                lastWasComma = true; // Mark last token as comma
                            }
                            else
                            {
                                break;
                            }
                        }

                        // If the last token before ':' was a comma (e.g., var2,:N), issue a warning
                        if (lastWasComma)
                        {
                            Warnings.Add(new AnalyzerWarning(
                                "107", // Unique ID for trailing comma
                                string.Format(MessageRegistry.GetMessage(107).Text, CurrentToken.TokenLine, CurrentToken.TokenColumn - 1)));
                        }

                        // 3) Step: Create separator expression
                        SeparatorExpression separator = new (CurrentToken);
                        Eat(CurrentToken, "Keyword");

                        // 4) Step: Collect Specifier type expression until not find new semmicolon.
                        var typeTokens = Tokens.Skip(position)
                        .TakeWhile(t => t.TokenType != TokenType.Semicolon && t.TokenType != TokenType.EndOfInput)
                        .ToList();

                        position += typeTokens.Count + 1;

                        // Add remaining tokens to the end of the row. Support multiple declaration in one line.
                        if (CurrentToken.TokenType != TokenType.EndOfInput && typeTokens.Count > 0)
                        {
                            // Handle multiple separator warning:
                            // Skip over all leading ',' or ';' and add warnings
                            while (position < Tokens.Count &&
                                  (Tokens[position].TokenType == TokenType.Comma || Tokens[position].TokenType == TokenType.Semicolon))
                            {
                                var warningToken = Tokens[position];

                                Warnings.Add(new AnalyzerWarning(
                                        "108",
                                        string.Format(MessageRegistry.GetMessage(108).Text, CurrentToken.TokenLine, CurrentToken.TokenColumn - 1)));

                                position++; // Skip this token
                            }

                            var tokens = Tokens.Skip(position).ToList();
                            splitedTokens.Insert(i + 1, tokens);
                        }
                        else if (typeTokens.Count == 0)
                        {
                            // Error if separator not followed TypeSpecifierExpression
                            // Generate error message
                            throw new AnalyzerError(
                                "101",
                                string.Format(MessageRegistry.GetMessage(101).Text, Tokens[position - 1].TokenLine, Tokens[position - 1].TokenColumn, "TypeSpecifier ('[]' or ',' or ';')"));
                        }

                        // 5) Step: Create Typesepcification expression
                        TypeSpecifierExpression typeSpecifier = new (typeTokens);

                        // 6) Step: Create VariableDeclaration for each variable
                        foreach (var varExpr in variables)
                        {
                            VariableDeclaration variableDeclaration = new (varExpr, separator, typeSpecifier);

                            variableDeclarations.Add(variableDeclaration); // Add to the list of variable declarations
                        }

                        break;
                    }
                    while (false);
                }
                catch (Exception ex)
                {
                    if (ex is AnalyzerWarning warning)
                    {
                        Warnings.Add(warning);
                    }
                    else if (ex is AnalyzerError error)
                    {
                        Errors.Add(error);
                    }
                    else
                    {
                        // Fallback: Add generic errors to both lists if the type is unknown
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            // Store the parsed variable declarations in the Sections dictionary, grouped by the "StateSpace" macro
            Sections["StateSpace"] = variableDeclarations;
        }

        /// <summary>
        /// Parses the Input macro section, handling variable initialize.
        /// </summary>
        public static void InputParser()
        {
        }

        /// <summary>
        /// Parses the Precondition macro section, handling variable initialize.
        /// </summary>
        public static void PreconditionParser()
        {
        }

        /// <summary>
        /// Parses the Postcondition macro section, handling variable initialize.
        /// </summary>
        public static void PostconditionParser()
        {
        }

        /// <summary>
        /// Analyzes the provided tokenized sections, validating their syntactical structure and identifying
        /// any syntax errors or warnings. It processes the input and returns the result of the syntax analysis,
        /// indicating whether the code conforms to the expected grammatical rules.
        /// </summary>
        /// <param name="sections">A dictionary containing tokenized input grouped by sections. Each section represents
        /// a logical unit of code, such as a function, loop, or declaration, with the tokens for that section
        /// organized in a list.</param>
        /// <returns>
        /// A <see cref="ParserResult"/> containing:
        /// <list type="bullet">
        ///   <item><description><see cref="ParserResult.errors"/> – A list of syntax errors encountered during analysis.</description></item>
        ///   <item><description><see cref="ParserResult.warnings"/> – A list of non-critical syntax warnings that may need attention.</description></item>
        ///   <item><description><see cref="ParserResult.sections"/> – A dictionary mapping macro section names to their associated parsed statements.</description></item>
        ///   <item><description><see cref="ParserResult.isSuccessful"/> – A boolean indicating whether the analysis passed successfully without critical errors.</description></item>
        /// </list>
        /// </returns>
        public ParserResult Analyze(Dictionary<string, List<Token>> sections)
        {
            Reset();

            foreach (var macro in MacroType.ExpectedMacros)
            {
                var macroString = macro.Value; // Get the string value of the macro
                Tokens.Clear();
                Tokens.AddRange(sections[macroString].ToList());

                // Call the correct parsing function based on macroKey
                switch (macroString)
                {
                    case "StateSpace":
                        StatePlaceParser();
                        break;
                    case "Input":
                        InputParser();
                        break;
                    case "Precondition":
                        PreconditionParser();
                        break;
                    case "Postcondition":
                        PostconditionParser();
                        break;
                    default:
                        throw new Exception();
                }
            }

            // Return results
            bool isSuccessful = Errors.Count == 0;
            return new ParserResult(Errors, Warnings, Sections, isSuccessful);
        }

        /// <summary>
        /// Splits a flat list of tokens into rows of tokens separated by newlines,
        /// removing whitespace and comment tokens.
        /// </summary>
        /// <param name="tokens">The input token list.</param>
        /// <returns>A list of token rows (line-based).</returns>
        private static List<List<Token>> SplitTokensByNewline(List<Token> tokens)
        {
            return tokens
                .Aggregate(new List<List<Token>> { new () }, (acc, token) =>
                {
                    if (token.TokenType == TokenType.Newline)
                    {
                        acc.Add([]); // Start a new group
                    }
                    else if (token.TokenType != TokenType.Whitespace &&
                             token.TokenType != TokenType.Comment) // Skip Whitespace and Comment tokens
                    {
                        acc.Last().Add(token); // Add token to the current group
                    }

                    return acc;
                })
                .Where(group => group.Count > 0) // Remove empty groups
                .ToList();
        }

        /// <summary>
        /// Consumes the current token and advances the token position.
        /// If the next token is null or marks the end of input unexpectedly, a warning is added.
        /// </summary>
        /// <param name="token">The current token to be consumed.</param>
        /// <param name="expressiontype">The expected expression type following the current token.</param>
        private static void Eat(Token token, string expressiontype)
        {
            if (PeekToken().TokenType == TokenType.EndOfInput)
            {
                // Generate error message
                throw new AnalyzerError(
                    "101",
                    string.Format(MessageRegistry.GetMessage(101).Text, token.TokenLine, token.TokenColumn, expressiontype));
            }

            position++;
        }

        /// <summary>
        /// Peeks ahead at a future token in the stream without advancing the current position.
        /// </summary>
        /// <param name="offset">The number of tokens to look ahead.</param>
        /// <returns>The peeked token.</returns>
        private static Token PeekToken(int offset = 1)
        {
            int peekPosition = position + offset;
            return peekPosition < Tokens.Count ? Tokens[peekPosition] : new Token { TokenType = TokenType.EndOfInput };
        }
    }
}
