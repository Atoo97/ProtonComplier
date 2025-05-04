// <copyright file="SemanticAnalyzer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
    using System.Text.RegularExpressions;
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;
    using Proton.Parser;
    using Proton.Parser.Statements;
    using Proton.Semantic.Interfaces;

    /// <summary>
    /// Represents the main sematical class responsible for analyzing tokens from each macro section,
    /// generating symbol table, and reporting errors and warnings.
    /// </summary>
    public partial class SemanticAnalyzer : ISemanticAnalyzer
    {
        // Stores statement grouped by macro sections
        private static readonly Dictionary<string, List<Statement>> Sections = new ();

        private static readonly SymbolTable SymbolTable = new ();
        private static readonly List<Statement> Statements = new ();

        // List to store errors and warnings
        private static readonly List<BaseException> Errors = new ();
        private static readonly List<BaseException> Warnings = new ();

        /// <summary>
        /// Before starting a new semantic analysis, reset everything.
        /// </summary>
        public static void Reset()
        {
            Sections.Clear();
            SymbolTable.Clear();
            Errors.Clear();
            Warnings.Clear();
        }

        /// <summary>
        /// Semantically analyzes the StateSpace macro section, handling variable declarations.
        /// This includes verifying identifiers, checking for duplicates, validating type specifiers,
        /// and populating the symbol table.
        /// </summary>
        public static void StatePlaceSemantical()
        {
            foreach (var statement in Statements)
            {
                try
                {
                    if (statement is VariableDeclaration variableDeclaration)
                    {
                        // Check if the identifier matches the valid naming rule
                        var match = IdentifierRegex().Match(statement.LeftNode!.ParseSymbol!.TokenValue);
                        if (!match.Success)
                        {
                            // Generate error message
                            throw new AnalyzerError(
                                "204",
                                string.Format(MessageRegistry.GetMessage(204).Text, statement.LeftNode!.ParseSymbol!.TokenValue, statement.LeftNode!.ParseSymbol!.TokenLine, statement.LeftNode!.ParseSymbol!.TokenColumn));
                        }

                        // Create new Symbol
                        var symbol = new Symbol
                        {
                            Name = statement.LeftNode.ParseSymbol!.TokenValue,
                            Type = statement.RightNode!.ParseSymbol!.TokenType,
                            Category = statement.LeftNode.ParseSymbol!.TokenCategory,
                            Value = new (),
                            SymbolLine = statement.LeftNode.ParseSymbol!.TokenLine,
                            SymbolColumn = statement.LeftNode.ParseSymbol!.TokenColumn,
                            IsList = variableDeclaration.IsList,
                        };

                        // Add symbol to symbol table if possible
                        SymbolTable.AddSymbol(symbol);
                    }
                    else
                    {
                        // Add error if not statement..
                    }
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
        }

        /// <summary>
        /// Analyzes the provided parsed statement sections, performing semantic validation such as
        /// type checking, symbol resolution, and logical rule enforcement. It ensures that the program's logic
        /// adheres to the defined language semantics.
        /// </summary>
        /// <param name="sections">
        /// A dictionary containing parsed statements grouped by macro sections. Each section represents
        /// a logical block of code (e.g., StateSpace, Input, Precondition), with its corresponding list of statements.
        /// </param>
        /// <returns>
        /// A <see cref="SemanticResult"/> containing:
        /// <list type="bullet">
        ///   <item><description><see cref="SemanticResult.errors"/> – A list of semantic errors found during analysis, such as undeclared variables or type mismatches.</description></item>
        ///   <item><description><see cref="SemanticResult.warnings"/> – A list of semantic warnings for non-critical issues, like unused variables or unreachable logic.</description></item>
        ///   <item><description><see cref="SemanticResult.sections"/> – A dictionary mapping macro section names to their associated parsed statements.</description></item>
        ///   <item><description><see cref="SemanticResult.table"/> – A symbol table mapping identifiers to their semantic information (e.g., type, scope, and location).</description></item>
        ///   <item><description><see cref="SemanticResult.isSuccessful"/> – A boolean indicating whether the semantic analysis completed without any critical errors.</description></item>
        /// </list>
        /// </returns>
        public SemanticResult Analyze(Dictionary<string, List<Statement>> sections)
        {
            Reset();

            foreach (var macro in MacroType.ExpectedMacros)
            {
                var macroString = macro.Value; // Get the string value of the macro
                Statements.Clear();
                if (sections.TryGetValue(macroString, out var statementList) && statementList is not null)
                {
                    Statements.AddRange(statementList.ToList());
                }
                else
                {
                    continue;
                }

                // Call the correct parsing function based on macroKey
                switch (macroString)
                {
                    case "StateSpace":
                        StatePlaceSemantical();
                        break;
                    case "Input":
                        // InputParser();
                        break;
                    case "Precondition":
                        // PreconditionParser();
                        break;
                    case "Postcondition":
                        // PostconditionParser();
                        break;
                    default:
                        throw new Exception();
                }
            }

            // Return results
            bool isSuccessful = Errors.Count == 0;
            return new SemanticResult(Errors, Warnings, Sections, SymbolTable, isSuccessful);
        }

        /// <summary>
        /// Compares the declared type of a symbol with the type specified in an expression.
        /// </summary>
        /// <param name="symbol">The declared symbol containing the semantic type (e.g., "Natural", "Real").</param>
        /// <param name="token">The expression containing the actual value type (e.g., "uint", "double").</param>
        /// <returns>True if types semantically match; otherwise, false.</returns>
        private static bool TypeMatch(Symbol symbol, Token token)
        {
            // Normalize expected and actual types
            var expectedType = symbol.Type;
            var actualType = token.TokenType;

            // Semantic mapping
            return (expectedType == TokenType.Natural && actualType == TokenType.Uint) ||
                   (expectedType == TokenType.Integer && actualType == TokenType.Int) ||
                   (expectedType == TokenType.Real && actualType == TokenType.Double) ||
                   (expectedType == TokenType.Boolean && actualType == TokenType.Bool) ||
                   (expectedType == TokenType.Character && actualType == TokenType.Char) ||
                   (expectedType == TokenType.Text && actualType == TokenType.String);
        }

        // Basic (stricter) concept of variable name regex, Cant be longer than 511 character
        [GeneratedRegex(@"\G([a-z_][\p{L}0-9_]{0,510})", RegexOptions.Compiled)]
        private static partial Regex IdentifierRegex();
    }
}
