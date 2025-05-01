// <copyright file="SemanticAnalyzer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
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
    public class SemanticAnalyzer : ISemanticAnalyzer
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
                // Statements.AddRange(sections[macroString].ToList()); now its null!

                // Call the correct parsing function based on macroKey
                switch (macroString)
                {
                    case "StateSpace":
                        // StatePlaceParser();
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

                // Just until first iteration (Delet lateer!!!)
                /*
                if (macroString == "Precondition")
                {
                    break;
                }
                */
            }

            // Return results
            bool isSuccessful = Errors.Count == 0;
            return new SemanticResult(Errors, Warnings, Sections, SymbolTable, isSuccessful);
        }

    }
}
