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
    using Proton.Parser.Expressions;
    using Proton.Parser.Statements;
    using Proton.Semantic.Interfaces;
    using ProtonComplier.Parser.Expressions;
    using ProtonComplier.Parser.Statements;

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
        /// Semantically analyzes the Input macro section, initialize all declared input variables.
        /// This includes checking identifier naming rules, ensuring type correctness,
        /// preventing duplicates, and inserting valid symbols into the symbol table.
        /// All semantic errors or warnings encountered during the analysis are recorded.
        /// </summary>
        public static void InputSemantical()
        {
            foreach (var statement in Statements)
            {
                try
                {
                    // 1) Chehck if varibale is exist in symboltable
                    Symbol symbol = SymbolTable.FindSymbol(statement.LeftNode!.ParseSymbol) !;

                    if (statement is VariableInitialization variableInitialization)
                    {
                        // 2) Chehck if defined as list or not
                        if (symbol.IsList != variableInitialization.IsList)
                        {
                            var token = variableInitialization.LeftNode!.ParseSymbol;
                            string declaredAs = symbol.IsList ? "a list" : "not a list";

                            throw new AnalyzerError(
                                "212",
                                string.Format(MessageRegistry.GetMessage(212).Text, symbol.Name, declaredAs, token.TokenLine, token.TokenColumn));
                        }

                        // 3) Add values to the Symbol, plus chehck typecorrectness
                        if (variableInitialization.IsList)
                        {
                            symbol.Value.Clear();
                            var listexpr = variableInitialization.RightNode as ListExpression;

                            if (listexpr!.ParseSymbol.TokenType == TokenType.ValueSpecifier) // Empty list
                            {
                                // Add ?? as indicate empty list:
                                symbol.Value.Add(new Token
                                {
                                    TokenType = TokenType.QuestionMarks,
                                    TokenCategory = TokenCategory.Special,
                                    TokenValue = "??",
                                    TokenLine = 0,
                                    TokenColumn = 0,
                                });

                                symbol.IsInitialized = true;
                                continue;
                            }

                            foreach (var item in listexpr!.Elements)
                            {
                                ValidateAndCollectTokens(item, symbol);

                                // Add semicolon as list separator:
                                symbol.Value.Add(new Token
                                {
                                    TokenType = TokenType.Semicolon,
                                    TokenCategory = TokenCategory.Punctuator,
                                    TokenValue = ";",
                                    TokenLine = 0,
                                    TokenColumn = 0,
                                });
                            }

                            symbol.IsInitialized = true;
                        }
                        else
                        {
                            var rightnode = variableInitialization.RightNode as Expression;
                            symbol.Value.Clear();
                            ValidateAndCollectTokens(rightnode!, symbol);
                            symbol.IsInitialized = true;
                        }
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
                        InputSemantical();
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

        private static void ValidateAndCollectTokens(Expression expr, Symbol symbol)
        {
            switch (expr)
            {
                case OperandExpression operand:
                    Token token = operand.ParseSymbol!;
                    var type = GetSemanticType(operand.ParseSymbol);

                    // Typecheck
                    if (type == TokenType.Identifier)
                    {
                        // Get the varibale values and recall the validation:
                        Symbol antohersymbol = SymbolTable.FindSymbol(token) !;

                        if (!antohersymbol.IsInitialized)
                        {
                            throw new AnalyzerError(
                                 "227",
                                 string.Format(MessageRegistry.GetMessage(227).Text, token.TokenValue, token.TokenLine, token.TokenColumn));
                        }
                        else if (symbol.Type != antohersymbol.Type && symbol.Type != TokenType.Boolean)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, antohersymbol.Type, token.TokenLine, token.TokenColumn));
                        }
                        /*
                        else if (symbol.Type == TokenType.Boolean && (antohersymbol.Type == TokenType.Character || antohersymbol.Type == TokenType.Text)) // Boolean type only handles numbers os bool
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, antohersymbol.Type, token.TokenLine, token.TokenColumn));
                        }
                        */
                        else
                        {
                            foreach (var item in antohersymbol.Value)
                            {
                                symbol.Value.Add(item);
                            }

                            break;
                        }
                    }
                    else if (symbol.Type == TokenType.Real)
                    {
                    }
                    else if (symbol.Type == TokenType.Integer)
                    {
                        if (type == TokenType.Real)
                        {
                            throw new AnalyzerError(
                                "232",
                                string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, token.TokenType, token.TokenLine, token.TokenColumn));
                        }
                    }
                    else if (symbol.Type == TokenType.Natural)
                    {
                        if (type == TokenType.Real || type == TokenType.Integer)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, token.TokenType, token.TokenLine, token.TokenColumn));
                        }
                    }
                    else if (symbol.Type == TokenType.Boolean)
                    {
                        // Nothing should happen here
                    }
                    else if (symbol.Type != type)
                    {
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, token.TokenType, token.TokenLine, token.TokenColumn));
                    }

                    symbol.Value.Add(token);
                    break;

                case OperatorExpression opExpr:
                    Token token2 = opExpr.ParseSymbol!;

                    // Chehck valid operator:
                    if (symbol.Type == TokenType.Boolean)
                    {
                        if (!IsBooleanOperator(token2.TokenType))
                        {
                            throw new AnalyzerError(
                               "234",
                               string.Format(MessageRegistry.GetMessage(234).Text, token2.TokenType, symbol.Type, token2.TokenLine, token2.TokenColumn));
                        }
                    }
                    else if (symbol.Type == TokenType.Character) // Cannot be applied operator for char
                    {
                        throw new AnalyzerError(
                              "234",
                              string.Format(MessageRegistry.GetMessage(234).Text, token2.TokenType, symbol.Type, token2.TokenLine, token2.TokenColumn));
                    }
                    else if (symbol.Type == TokenType.Text)
                    {
                        if (!IsStringOperator(token2.TokenType))
                        {
                            throw new AnalyzerError(
                               "234",
                               string.Format(MessageRegistry.GetMessage(234).Text, token2.TokenType, symbol.Type, token2.TokenLine, token2.TokenColumn));
                        }
                    }
                    else
                    {
                        if (!IsMathOperator(token2.TokenType))
                        {
                            throw new AnalyzerError(
                               "234",
                               string.Format(MessageRegistry.GetMessage(234).Text, token2.TokenType, symbol.Type, token2.TokenLine, token2.TokenColumn));
                        }
                    }

                    symbol.Value.Add(token2);
                    break;

                case BinaryExpression binExpr:
                    ValidateAndCollectTokens(binExpr.Left, symbol);
                    ValidateAndCollectTokens(binExpr.Operator, symbol);
                    ValidateAndCollectTokens(binExpr.Right, symbol);
                    break;
                case ParenthesisExpression parenExpr:
                    // Add parenthesises:
                    symbol.Value.Add(new Token
                    {
                        TokenType = TokenType.OpenParen,
                        TokenCategory = TokenCategory.Punctuator,
                        TokenValue = "(",
                        TokenLine = parenExpr.ParseSymbol.TokenLine,
                        TokenColumn = parenExpr.ParseSymbol.TokenColumn - 1,
                    });

                    ValidateAndCollectTokens(parenExpr.InnerExpression, symbol);

                    symbol.Value.Add(new Token
                    {
                        TokenType = TokenType.CloseParen,
                        TokenCategory = TokenCategory.Punctuator,
                        TokenValue = ")",
                        TokenLine = parenExpr.ParseSymbol.TokenLine,
                        TokenColumn = parenExpr.LastToken.TokenColumn + 1,
                    });

                    break;
            }
        }

        /// <summary>
        /// Infers the declared semantic type from a token's actual value type.
        /// </summary>
        /// <param name="token">The token containing the actual data type.</param>
        /// <returns>The corresponding declared semantic type.</returns>
        private static TokenType GetSemanticType(Token token)
        {
            return token.TokenType switch
            {
                TokenType.Uint => TokenType.Natural,
                TokenType.Int => TokenType.Integer,
                TokenType.Double => TokenType.Real,
                TokenType.Bool => TokenType.Boolean,
                TokenType.Char => TokenType.Character,
                TokenType.String => TokenType.Text,
                TokenType.Identifier => TokenType.Identifier,
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Determines whether the given TokenType is a valid math operator.
        /// </summary>
        private static bool IsMathOperator(TokenType type)
        {
            return type == TokenType.Addition ||
                   type == TokenType.Subtraction ||
                   type == TokenType.Multiplication ||
                   type == TokenType.Division ||
                   type == TokenType.Modulus;
        }

        /// <summary>
        /// Determines whether the given TokenType is a valid boolean operator.
        /// </summary>
        private static bool IsBooleanOperator(TokenType type)
        {
            return type == TokenType.Equal ||
                   type == TokenType.NotEqual ||
                   type == TokenType.GreaterThan ||
                   type == TokenType.LessThan ||
                   type == TokenType.GreaterThanOrEqual ||
                   type == TokenType.LessThanOrEqual ||
                   type == TokenType.LogicalAnd ||
                   type == TokenType.LogicalOr ||
                   type == TokenType.LogicalNot;
        }

        /// <summary>
        /// Determines whether the given TokenType is a valid string operator.
        /// </summary>
        private static bool IsStringOperator(TokenType type)
        {
            return type == TokenType.Addition; // for concatenation
        }

        // Basic (stricter) concept of variable name regex, Cant be longer than 511 character
        [GeneratedRegex(@"\G([a-z_][\p{L}0-9_]{0,510})", RegexOptions.Compiled)]
        private static partial Regex IdentifierRegex();
    }
}
