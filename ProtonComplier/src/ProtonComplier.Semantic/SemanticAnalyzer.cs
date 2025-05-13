// <copyright file="SemanticAnalyzer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;
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
        private static readonly List<Statement> Statements = new ();

        // List to store errors and warnings
        private static readonly List<BaseException> Errors = new ();
        private static readonly List<BaseException> Warnings = new ();

        private static SymbolTable symbolTable = new ();

        /// <summary>
        /// Before starting a new semantic analysis, reset everything.
        /// </summary>
        public static void Reset()
        {
            Sections.Clear();
            symbolTable.Clear();
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
                        symbolTable.AddSymbol(symbol);
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
            // Ensure the order of names follows the order in symbolTable.symbols
            SymbolTable newSymbolTable = new ();

            foreach (var statement in Statements)
            {
                try
                {
                    // 1) Chehck if varibale is exist in symboltable
                    Symbol symbol = symbolTable.FindSymbol(statement.LeftNode!.ParseSymbol) !;
                    newSymbolTable.AddSymbol(symbol);

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

                        // 3) Add values to the Symbol, plus check typecorrectness
                        if (variableInitialization.IsList)
                        {
                            if (symbol.IsInitialized)
                            {
                                throw new AnalyzerError(
                                   "207",
                                   string.Format(MessageRegistry.GetMessage(207).Text, symbol.Name, symbol.SymbolLine, symbol.SymbolColumn));
                            }

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

                                if (symbol.Type == TokenType.Boolean)
                                {
                                    string valueStr = symbol.ValueTokens.ToString();
                                    int lastCommaIndex = valueStr.LastIndexOf(',');

                                    string lastSegment = lastCommaIndex >= 0
                                        ? valueStr.Substring(lastCommaIndex + 1).Trim()
                                        : valueStr.Trim();

                                    // Check if boolean type is valid
                                    if (!IsValidExpression(lastSegment, "bool"))
                                    {
                                        throw new AnalyzerError(
                                             "239",
                                             string.Format(MessageRegistry.GetMessage(239).Text, lastSegment, symbol.SymbolLine, symbol.SymbolColumn));
                                    }
                                }

                                symbol.ValueTokens.Append(',');

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
                            if (symbol.IsInitialized)
                            {
                                throw new AnalyzerError(
                                   "207",
                                   string.Format(MessageRegistry.GetMessage(207).Text, symbol.Name, symbol.SymbolLine, symbol.SymbolColumn));
                            }

                            ValidateAndCollectTokens(rightnode!, symbol);

                            if (symbol.Type == TokenType.Boolean)
                            {
                                string valueStr = symbol.ValueTokens.ToString();
                                int lastCommaIndex = valueStr.LastIndexOf(',');

                                string lastSegment = lastCommaIndex >= 0
                                    ? valueStr.Substring(lastCommaIndex + 1).Trim()
                                    : valueStr.Trim();

                                // Check if boolean type is valid
                                if (!IsValidExpression(lastSegment, "bool"))
                                {
                                    throw new AnalyzerError(
                                         "239",
                                         string.Format(MessageRegistry.GetMessage(239).Text, lastSegment, symbol.SymbolLine, symbol.SymbolColumn));
                                }
                            }

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

            foreach (var symbol in symbolTable.Symbols)
            {
                if (!newSymbolTable.Symbols.Any(s => s.Name == symbol.Name))
                {
                    newSymbolTable.AddSymbol(symbol);
                }
            }

            symbolTable.Clear();
            symbolTable = newSymbolTable;
        }

        /// <summary>
        /// Semantically analyzes the Precondition macro section.
        /// This includes checking if expression is boolean.
        /// All semantic errors or warnings encountered during the analysis are recorded.
        /// </summary>
        public static void PreconditionParser()
        {
            foreach (var statement in Statements)
            {
                try
                {
                    // Create new fake boolean Symbol
                    var symbol = new Symbol
                    {
                        Name = "0",
                        Type = TokenType.Boolean,
                        Category = TokenCategory.Keyword,
                        Value = new (),
                        SymbolLine = -1,
                        SymbolColumn = -1,
                        IsList = false,
                    };

                    // Add symbol to symbol table if possible
                    symbolTable.AddSymbol(symbol);

                    if (statement is PreconditionDeclaration preconditionDeclaration)
                    {
                        var expr = preconditionDeclaration.RightNode as Expression;
                        var line = expr!.ParseSymbol.TokenLine;
                        var column = expr!.ParseSymbol.TokenColumn;
                        ValidateAndCollectTokens(expr!, symbol);
                        string valueStr = symbol.ValueTokens.ToString();

                        // Check if boolean type is valid
                        if (!IsValidExpression(valueStr, "bool"))
                        {
                            throw new AnalyzerError(
                                 "239",
                                 string.Format(MessageRegistry.GetMessage(239).Text, valueStr, line, column));
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
        /// Semantically analyzes the Postcondition macro section.
        /// This includes checking if expression is boolean.
        /// All semantic errors or warnings encountered during the analysis are recorded.
        /// </summary>
        public static void PostconditionParser()
        {
            int s = 0;

            // Create new fake condition Symbol
            var implicationsymbol = new Symbol
            {
                Name = "0C",
                Type = TokenType.Boolean,
                Category = TokenCategory.Keyword,
                Value = new (),
                SymbolLine = -1,
                SymbolColumn = -1,
                IsList = false,
                IsResult = false,
            };

            // Add symbol to symbol table if possible
            symbolTable.AddSymbol(implicationsymbol);

            foreach (var statement in Statements)
            {
                try
                {
                    if (statement is PostconditionImplication postcondition)
                    {
                        // Create new fake condition Symbol
                        var symbol = new Symbol
                        {
                            Name = "0C_Fake",
                            Type = TokenType.Boolean,
                            Category = TokenCategory.Keyword,
                            Value = new (),
                            SymbolLine = -1,
                            SymbolColumn = -1,
                            IsList = false,
                            IsResult = false,
                        };

                        var expr = postcondition.LeftNode as Expression;
                        var line = expr!.ParseSymbol.TokenLine;
                        var column = expr!.ParseSymbol.TokenColumn;

                        if (!implicationsymbol.IsResult)
                        {
                            implicationsymbol.ValueTokens.Append("if");
                        }
                        else
                        {
                            implicationsymbol.ValueTokens.Append("else if");
                        }

                        ValidateAndCollectTokens(expr!, symbol);
                        string valueStr = symbol.ValueTokens.ToString();

                        // Check if boolean type is valid
                        if (!IsValidExpression(valueStr, "bool"))
                        {
                            throw new AnalyzerError(
                                 "239",
                                 string.Format(MessageRegistry.GetMessage(239).Text, valueStr, line, column));
                        }

                        implicationsymbol.IsResult = true;
                        implicationsymbol.ValueTokens.Append(symbol.ValueTokens);
                        implicationsymbol.ValueTokens.Append('|');

                        List<Token> variables = new ();
                        foreach (var variableInitialization in postcondition.Initializations)
                        {
                            // variables.Add(variableInitialization.LeftNode!.ParseSymbol);

                            // 1) Chehck if varibale is exist in symboltable
                            Symbol symbol2 = symbolTable.FindSymbol(variableInitialization.LeftNode!.ParseSymbol) !;

                            if (symbol2.IsInitialized)
                            {
                                // Generate unique name with increment
                                string newName = $"{s}_{symbol2.Name}";
                                Symbol newSymbol = new Symbol
                                {
                                    Name = newName, // set the name with incremented suffix
                                    Category = symbol2.Category,
                                    Type = symbol2.Type,
                                    Value = new (), // again assuming Token has a copy constructor
                                    SymbolLine = symbol2.SymbolLine,
                                    SymbolColumn = symbol2.SymbolColumn,
                                    IsList = symbol2.IsList,
                                    IsInitialized = false,
                                    IsResult = true,
                                };

                                // symbol2.IsInitialized = false;
                                variableInitializationHelper(newSymbol, variableInitialization);

                                variables.Add(new Token
                                {
                                    TokenType = variableInitialization.LeftNode!.ParseSymbol.TokenType,
                                    TokenCategory = variableInitialization.LeftNode!.ParseSymbol.TokenCategory,
                                    TokenValue = newName,
                                    TokenLine = variableInitialization.LeftNode!.ParseSymbol.TokenLine,
                                    TokenColumn = variableInitialization.LeftNode!.ParseSymbol.TokenColumn,
                                });

                                // Add symbol to symbol table if possible
                                symbolTable.AddSymbol(newSymbol);

                                s++;
                            }
                            else
                            {
                                variableInitializationHelper(symbol2, variableInitialization);
                                variables.Add(variableInitialization.LeftNode!.ParseSymbol);
                            }
                        }

                        implicationsymbol.Value.AddRange(variables);
                        implicationsymbol.ValueTokens.Append($"{variables.Count}");
                        implicationsymbol.ValueTokens.Append('|');
                    }
                    else if (statement is VariableInitialization variableInitialization)
                    {
                        // 1) Chehck if varibale is exist in symboltable
                        Symbol symbol = symbolTable.FindSymbol(statement.LeftNode!.ParseSymbol) !;
                        variableInitializationHelper(symbol, variableInitialization);
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

            // if 0C is result:
            if (implicationsymbol.IsResult)
            {
                foreach (var token in implicationsymbol.Value)
                {
                    Symbol symbol = symbolTable.FindSymbol(token) !;
                    symbol.IsResult = false;
                    symbol.IsInitialized = false;
                }
            }
        }

        public static void variableInitializationHelper(Symbol symbol, VariableInitialization variableInitialization)
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

            // 3) Add values to the Symbol, plus check typecorrectness
            if (variableInitialization.IsList)
            {
                if (symbol.IsInitialized)
                {
                    throw new AnalyzerError(
                       "207",
                       string.Format(MessageRegistry.GetMessage(207).Text, symbol.Name, symbol.SymbolLine, symbol.SymbolColumn));
                }

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
                    symbol.IsResult = true;
                    return;
                }

                foreach (var item in listexpr!.Elements)
                {
                    ValidateAndCollectTokens(item, symbol);

                    if (symbol.Type == TokenType.Boolean)
                    {
                        string valueStr = symbol.ValueTokens.ToString();
                        int lastCommaIndex = valueStr.LastIndexOf(',');

                        string lastSegment = lastCommaIndex >= 0
                            ? valueStr.Substring(lastCommaIndex + 1).Trim()
                            : valueStr.Trim();

                        // Check if boolean type is valid
                        if (!IsValidExpression(lastSegment, "bool"))
                        {
                            throw new AnalyzerError(
                                 "239",
                                 string.Format(MessageRegistry.GetMessage(239).Text, lastSegment, symbol.SymbolLine, symbol.SymbolColumn));
                        }
                    }

                    symbol.ValueTokens.Append(',');

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
                symbol.IsResult = true;
            }
            else
            {
                var rightnode = variableInitialization.RightNode as Expression;
                if (symbol.IsInitialized)
                {
                    throw new AnalyzerError(
                       "207",
                       string.Format(MessageRegistry.GetMessage(207).Text, symbol.Name, symbol.SymbolLine, symbol.SymbolColumn));
                }

                ValidateAndCollectTokens(rightnode!, symbol);

                if (symbol.Type == TokenType.Boolean)
                {
                    string valueStr = symbol.ValueTokens.ToString();
                    int lastCommaIndex = valueStr.LastIndexOf(',');

                    string lastSegment = lastCommaIndex >= 0
                        ? valueStr.Substring(lastCommaIndex + 1).Trim()
                        : valueStr.Trim();

                    // Check if boolean type is valid
                    if (!IsValidExpression(lastSegment, "bool"))
                    {
                        throw new AnalyzerError(
                             "239",
                             string.Format(MessageRegistry.GetMessage(239).Text, lastSegment, symbol.SymbolLine, symbol.SymbolColumn));
                    }
                }

                symbol.IsInitialized = true;
                symbol.IsResult = true;
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
            return new SemanticResult(Errors, Warnings, Sections, symbolTable, isSuccessful);
        }

        private static void ValidateAndCollectTokens(Expression expr, Symbol symbol)
        {
            Token token;
            TokenType symbolType;
            switch (expr)
            {
                case ParenthesisExpression parenExpr:
                    symbol.Value.Add(new Token // Add parenthesis:
                    {
                        TokenType = TokenType.OpenParen,
                        TokenCategory = TokenCategory.Punctuator,
                        TokenValue = "(",
                        TokenLine = parenExpr.ParseSymbol.TokenLine,
                        TokenColumn = parenExpr.ParseSymbol.TokenColumn - 1,
                    });
                    symbol.ValueTokens.Append('(');

                    ValidateAndCollectTokens(parenExpr.InnerExpression, symbol);

                    symbol.Value.Add(new Token // Add parenthesis:
                    {
                        TokenType = TokenType.CloseParen,
                        TokenCategory = TokenCategory.Punctuator,
                        TokenValue = ")",
                        TokenLine = parenExpr.ParseSymbol.TokenLine,
                        TokenColumn = parenExpr.LastToken.TokenColumn + 1,
                    });
                    symbol.ValueTokens.Append(')');

                    break;
                case LengthExpression lenExpr: // Identifier can be only list of any tpye or text type variable
                    // Get the varibale values and recall the validation:
                    Symbol anothersymbol = symbolTable.FindSymbol(lenExpr.Identifier.ParseSymbol) !;
                    if (!anothersymbol.IsList && anothersymbol.Type != TokenType.Text)
                    {
                        // Not a list and not Text type — invalid usage
                        throw new AnalyzerError(
                            "232",
                            string.Format(
                                MessageRegistry.GetMessage(232).Text,
                                "List or Text",
                                anothersymbol.Type,
                                lenExpr.Identifier.ParseSymbol.TokenLine,
                                lenExpr.Identifier.ParseSymbol.TokenColumn));
                    }

                    symbol.Value.Add(lenExpr.Identifier.ParseSymbol);
                    var tokenValue = lenExpr.Identifier.ParseSymbol.TokenValue;
                    symbol.ValueTokens.Append($"{tokenValue}");
                    symbol.Value.Add(lenExpr.ParseSymbol);
                    symbol.ValueTokens.Append('.');
                    symbol.Value.Add(lenExpr.Lenght);
                    symbol.ValueTokens.Append($"Length");
                    break;
                case BinaryExpression binExpr:
                    ValidateAndCollectTokens(binExpr.Left, symbol);
                    ValidateAndCollectTokens(binExpr.Operator, symbol);
                    ValidateAndCollectTokens(binExpr.Right, symbol);
                    break;
                case ListNthElementExpression listNExp:
                    symbol.ValueTokens.Append(listNExp.Identifier.ParseSymbol.TokenValue);
                    symbol.ValueTokens.Append('[');

                    if (listNExp.Operand is OperandExpression)
                    {
                        ValidateIdentifierTokens(listNExp.Identifier.ParseSymbol, symbol, listNExp.Operand.ParseSymbol);
                    }
                    else if (listNExp.Operand is BinaryExpression)
                    {
                        ValidateAndCollectTokens(listNExp.Operand, symbol);
                    }
                    else
                    {
                        // Error invalid index:
                        throw new AnalyzerError(
                              "250",
                              string.Format(MessageRegistry.GetMessage(250).Text, listNExp.Operand.ParseSymbol.TokenLine, listNExp.Operand.ParseSymbol.TokenColumn));
                    }

                    symbol.ValueTokens.Append(']');
                    break;
                case MaxExpression maxExpr:
                    symbol.Value.Add(new Token // Add max:
                    {
                        TokenType = TokenType.Max,
                        TokenCategory = TokenCategory.Keyword,
                        TokenValue = "Max",
                        TokenLine = maxExpr.ParseSymbol.TokenLine,
                        TokenColumn = maxExpr.ParseSymbol.TokenColumn - 1,
                    });

                    symbol.ValueTokens.Append("Math.Max");
                    symbol.ValueTokens.Append('(');
                    ValidateAndCollectTokens(maxExpr.LeftExpression, symbol);
                    symbol.ValueTokens.Append(',');
                    ValidateAndCollectTokens(maxExpr.RightExpression, symbol);
                    symbol.ValueTokens.Append(')');
                    break;
                case MinExpression minExpr:
                    symbol.Value.Add(new Token // Add min:
                    {
                        TokenType = TokenType.Max,
                        TokenCategory = TokenCategory.Keyword,
                        TokenValue = "Min",
                        TokenLine = minExpr.ParseSymbol.TokenLine,
                        TokenColumn = minExpr.ParseSymbol.TokenColumn - 1,
                    });

                    symbol.ValueTokens.Append("Math.Min");
                    symbol.ValueTokens.Append('(');
                    ValidateAndCollectTokens(minExpr.LeftExpression, symbol);
                    symbol.ValueTokens.Append(',');
                    ValidateAndCollectTokens(minExpr.RightExpression, symbol);
                    symbol.ValueTokens.Append(')');
                    break;
                case OperatorExpression opExpr: // Check valid operator type
                    token = opExpr.ParseSymbol!;
                    symbolType = symbol.Type;

                    if (symbolType == TokenType.Character) // Cannot be applied operator for char
                    {
                        throw new AnalyzerError(
                              "234",
                              string.Format(MessageRegistry.GetMessage(234).Text, token.TokenType, symbol.Type, token.TokenLine, token.TokenColumn));
                    }
                    else if (symbolType == TokenType.Text)
                    {
                        if (!IsStringOperator(token.TokenType))
                        {
                            throw new AnalyzerError(
                               "234",
                               string.Format(MessageRegistry.GetMessage(234).Text, token.TokenType, symbol.Type, token.TokenLine, token.TokenColumn));
                        }
                    }
                    else if (symbolType == TokenType.Boolean) // mapping it into valid C# operator
                    {
                        var op = GetCSharpOperator(token.TokenType.ToString());
                        symbol.Value.Add(token);
                        if (op.Item1)
                        {
                            symbol.ValueTokens.Append(" " + op.Item2 + " ");
                        }
                        else
                        {
                            symbol.ValueTokens.Append(" " + token.TokenValue + " ");
                        }

                        break;
                    }
                    else
                    {
                        if (!IsMathOperator(token.TokenType))
                        {
                            throw new AnalyzerError(
                               "234",
                               string.Format(MessageRegistry.GetMessage(234).Text, token.TokenType, symbol.Type, token.TokenLine, token.TokenColumn));
                        }
                    }

                    symbol.Value.Add(token);
                    symbol.ValueTokens.Append(token.TokenValue);
                    break;
                case OperandExpression operand:
                    Token operandtoken = operand.ParseSymbol!;
                    TokenType operandtype = GetSemanticType(operand.ParseSymbol);
                    symbolType = symbol.Type;

                    if (symbolType == TokenType.Character)
                    {
                        if (operandtype != TokenType.Character && operandtype != TokenType.Identifier)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, operandtoken.TokenType, operandtoken.TokenLine, operandtoken.TokenColumn));
                        }

                        if (operandtype == TokenType.Character) // If char
                        {
                            symbol.Value.Add(operandtoken);

                            var c = $"{operandtoken.TokenValue}";
                            symbol.ValueTokens.Append(c);
                        }
                        else // If identifier
                        {
                            ValidateIdentifierTokens(operandtoken, symbol, null!);
                        }
                    }
                    else if (symbolType == TokenType.Text)
                    {
                        if (operandtype != TokenType.Text && operandtype != TokenType.Identifier)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, operandtoken.TokenType, operandtoken.TokenLine, operandtoken.TokenColumn));
                        }

                        if (operandtype == TokenType.Text) // If string
                        {
                            symbol.Value.Add(operandtoken);
                            symbol.ValueTokens.Append(operandtoken.TokenValue);
                        }
                        else // If identifier
                        {
                            ValidateIdentifierTokens(operandtoken, symbol, null!);
                        }
                    }
                    else if (symbolType == TokenType.Real)
                    {
                        if (operandtype != TokenType.Real && operandtype != TokenType.Integer && operandtype != TokenType.Natural && operandtype != TokenType.Identifier)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, operandtoken.TokenType, operandtoken.TokenLine, operandtoken.TokenColumn));
                        }

                        if (operandtype == TokenType.Real || operandtype == TokenType.Integer || operandtype == TokenType.Natural) // If Real or Integer or Naturaél
                        {
                            symbol.Value.Add(operandtoken);
                            symbol.ValueTokens.Append(operandtoken.TokenValue);
                        }
                        else // If identifier
                        {
                            ValidateIdentifierTokens(operandtoken, symbol, null!);
                        }
                    }
                    else if (symbolType == TokenType.Integer)
                    {
                        if (operandtype != TokenType.Integer && operandtype != TokenType.Natural && operandtype != TokenType.Identifier)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, operandtoken.TokenType, operandtoken.TokenLine, operandtoken.TokenColumn));
                        }

                        if (operandtype == TokenType.Integer || operandtype == TokenType.Natural) // If Integer or Natural
                        {
                            symbol.Value.Add(operandtoken);
                            symbol.ValueTokens.Append(operandtoken.TokenValue);
                        }
                        else // If identifier
                        {
                            ValidateIdentifierTokens(operandtoken, symbol, null!);
                        }
                    }
                    else if (symbolType == TokenType.Natural)
                    {
                        if (operandtype != TokenType.Natural && operandtype != TokenType.Identifier)
                        {
                            throw new AnalyzerError(
                                 "232",
                                 string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, operandtoken.TokenType, operandtoken.TokenLine, operandtoken.TokenColumn));
                        }

                        if (operandtype == TokenType.Natural) // If Natural
                        {
                            symbol.Value.Add(operandtoken);
                            symbol.ValueTokens.Append(operandtoken.TokenValue);
                        }
                        else // If identifier
                        {
                            ValidateIdentifierTokens(operandtoken, symbol, null!);
                        }
                    }
                    else if (symbolType == TokenType.Boolean)
                    {
                        if (operandtype == TokenType.Identifier) // If Identifier
                        {
                            ValidateIdentifierTokens(operandtoken, symbol, null!);
                        }
                        else if (operandtype == TokenType.Character)
                        {
                            symbol.Value.Add(operandtoken);

                            var c = $"{operandtoken.TokenValue}";
                            symbol.ValueTokens.Append(c);
                        }
                        else if (operandtype == TokenType.Boolean)
                        {
                            symbol.Value.Add(operandtoken);

                            // Change the first character to lower (True -> true)
                            var b = char.ToLower(operandtoken.TokenValue[0]) + operandtoken.TokenValue.Substring(1);
                            symbol.ValueTokens.Append(b);
                        }
                        else
                        {
                            symbol.Value.Add(operandtoken);
                            symbol.ValueTokens.Append(operandtoken.TokenValue);
                        }
                    }

                    break;
            }
        }

        private static void ValidateIdentifierTokens(Token operandtoken, Symbol symbol, Token nthElement)
        {
            // Get the varibale values and recall the validation:
            Symbol anothersymbol = symbolTable.FindSymbol(operandtoken) !;

            if (!anothersymbol.IsInitialized)
            {
                throw new AnalyzerError(
                     "227",
                     string.Format(MessageRegistry.GetMessage(227).Text, operandtoken.TokenValue, operandtoken.TokenLine, operandtoken.TokenColumn));
            }
            else if (anothersymbol.IsList && nthElement == null)
            {
                // List cannot be used!
                throw new AnalyzerError(
                    "232",
                    string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, "List(" + anothersymbol.Type + ")", operandtoken.TokenLine, operandtoken.TokenColumn));
            }
            else if (!anothersymbol.IsList && nthElement != null)
            {
                // Not a list cannot be used!
                throw new AnalyzerError(
                    "232",
                    string.Format(MessageRegistry.GetMessage(232).Text, "List(" + symbol.Type + ")", anothersymbol.Type, operandtoken.TokenLine, operandtoken.TokenColumn));
            }
            else if (anothersymbol.IsList && nthElement != null)
            {
                if (nthElement.TokenType == TokenType.Identifier) // Get identifier value
                {
                    // Get the varibale values and recall the validation:
                    Symbol indexsymbol = symbolTable.FindSymbol(nthElement) !;

                    // Chehck if index is Natural or Integer
                    if (indexsymbol.Type != TokenType.Natural && indexsymbol.Type != TokenType.Integer)
                    {
                        // Generate error message
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, "Natural or Integer", indexsymbol.Type, nthElement.TokenLine, nthElement.TokenColumn));
                    }
                    else if (indexsymbol.IsList)
                    {
                        // List cannot be used!
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, "List(" + anothersymbol.Type + ")", operandtoken.TokenLine, operandtoken.TokenColumn));
                    }

                    try
                    {
                        // int result = Convert.ToInt32(new DataTable().Compute(indexsymbol.ValueTokens, null));
                        int i = Convert.ToInt32(new DataTable().Compute(indexsymbol.ValueTokens.ToString(), null));

                        // Separate tokens into list according semmicolons
                        List<List<Token>> result = new ();
                        List<Token> currentGroup = new ();

                        foreach (var token in anothersymbol.Value)
                        {
                            if (token.TokenType == TokenType.Semicolon)
                            {
                                if (currentGroup.Count > 0)
                                {
                                    result.Add(new List<Token>(currentGroup));
                                    currentGroup.Clear();
                                }
                            }
                            else
                            {
                                currentGroup.Add(token);
                            }
                        }

                        // Add any remaining tokens as the last group
                        if (currentGroup.Count > 0)
                        {
                            result.Add(currentGroup);
                        }

                        if (i < 0)
                        {
                            i *= -1;
                            currentGroup = result[^i];
                            symbol.ValueTokens.Append($"^({nthElement.TokenValue}*-1)");
                        }
                        else
                        {
                            currentGroup = result[i];
                            symbol.ValueTokens.Append(nthElement.TokenValue);
                        }

                        // Add tokens
                        foreach (var item in currentGroup)
                        {
                            symbol.Value.Add(item);
                        }
                    }
                    catch (Exception)
                    {
                        // Over indexed
                        throw new AnalyzerError(
                            "244",
                            string.Format(MessageRegistry.GetMessage(244).Text, nthElement.TokenValue, nthElement.TokenLine, nthElement.TokenColumn));
                    }
                }
                else
                {
                    // Chehck if index is Natural or Integer
                    if (anothersymbol.Type != TokenType.Natural && anothersymbol.Type != TokenType.Integer && anothersymbol.Type != symbol.Type)
                    {
                        // Generate error message
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, "Natural or Integer", anothersymbol.Type, nthElement.TokenLine, nthElement.TokenColumn));
                    }

                    // Get N'th element value
                    try
                    {
                        int i = int.Parse(nthElement.TokenValue);

                        // Separate tokens into list according semmicolons
                        List<List<Token>> result = new ();
                        List<Token> currentGroup = new ();

                        foreach (var token in anothersymbol.Value)
                        {
                            if (token.TokenType == TokenType.Semicolon)
                            {
                                if (currentGroup.Count > 0)
                                {
                                    result.Add(new List<Token>(currentGroup));
                                    currentGroup.Clear();
                                }
                            }
                            else
                            {
                                currentGroup.Add(token);
                            }
                        }

                        // Add any remaining tokens as the last group
                        if (currentGroup.Count > 0)
                        {
                            result.Add(currentGroup);
                        }

                        if (i < 0)
                        {
                            i *= -1;
                            currentGroup = result[^i];
                            symbol.ValueTokens.Append($"^{i}");
                        }
                        else
                        {
                            currentGroup = result[i];
                            symbol.ValueTokens.Append(nthElement.TokenValue);
                        }

                        // Add tokens
                        foreach (var item in currentGroup)
                        {
                            symbol.Value.Add(item);
                        }
                    }
                    catch (Exception)
                    {
                        // Over indexed
                        throw new AnalyzerError(
                            "244",
                            string.Format(MessageRegistry.GetMessage(244).Text, int.Parse(nthElement.TokenValue), nthElement.TokenLine, nthElement.TokenColumn));
                    }
                }
            }
            else // If not a list
            {
                // Chehck type match:
                if (symbol.Type == TokenType.Character)
                {
                    if (anothersymbol.Type != TokenType.Character)
                    {
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, anothersymbol.Type, operandtoken.TokenLine, operandtoken.TokenColumn));
                    }
                    else
                    {
                        foreach (var item in anothersymbol.Value)
                        {
                            symbol.Value.Add(item);
                        }

                        // symbol.ValueTokens.Append(anothersymbol.ValueTokens);
                        symbol.ValueTokens.Append(anothersymbol.Name);
                    }
                }
                else if (symbol.Type == TokenType.Text)
                {
                    if (anothersymbol.Type != TokenType.Text)
                    {
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, anothersymbol.Type, operandtoken.TokenLine, operandtoken.TokenColumn));
                    }
                    else
                    {
                        foreach (var item in anothersymbol.Value)
                        {
                            symbol.Value.Add(item);
                        }

                        // symbol.ValueTokens.Append(anothersymbol.ValueTokens);
                        symbol.ValueTokens.Append(anothersymbol.Name);
                    }
                }
                else if (symbol.Type == TokenType.Real)
                {
                    if (anothersymbol.Type != TokenType.Real && anothersymbol.Type != TokenType.Natural && anothersymbol.Type != TokenType.Integer)
                    {
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, anothersymbol.Type, operandtoken.TokenLine, operandtoken.TokenColumn));
                    }
                    else
                    {
                        foreach (var item in anothersymbol.Value)
                        {
                            symbol.Value.Add(item);
                        }

                        // symbol.ValueTokens.Append(anothersymbol.ValueTokens);
                        symbol.ValueTokens.Append(anothersymbol.Name);
                    }
                }
                else if (symbol.Type == TokenType.Integer)
                {
                    if (anothersymbol.Type != TokenType.Natural && anothersymbol.Type != TokenType.Integer)
                    {
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, anothersymbol.Type, operandtoken.TokenLine, operandtoken.TokenColumn));
                    }
                    else
                    {
                        foreach (var item in anothersymbol.Value)
                        {
                            symbol.Value.Add(item);
                        }

                        // symbol.ValueTokens.Append(anothersymbol.ValueTokens);
                        symbol.ValueTokens.Append(anothersymbol.Name);
                    }
                }
                else if (symbol.Type == TokenType.Natural)
                {
                    if (anothersymbol.Type != TokenType.Natural)
                    {
                        throw new AnalyzerError(
                            "232",
                            string.Format(MessageRegistry.GetMessage(232).Text, symbol.Type, anothersymbol.Type, operandtoken.TokenLine, operandtoken.TokenColumn));
                    }
                    else
                    {
                        foreach (var item in anothersymbol.Value)
                        {
                            symbol.Value.Add(item);
                        }

                        // symbol.ValueTokens.Append(anothersymbol.ValueTokens);
                        symbol.ValueTokens.Append(anothersymbol.Name);
                    }
                }
                else if (symbol.Type == TokenType.Boolean)
                {
                    foreach (var item in anothersymbol.Value)
                    {
                        symbol.Value.Add(item);
                    }

                    // symbol.ValueTokens.Append(anothersymbol.ValueTokens);
                    symbol.ValueTokens.Append(anothersymbol.Name);
                }
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
        /// Converts a token type name to its corresponding C# operator.
        /// Returns a tuple indicating whether the conversion was successful and the resulting string.
        /// </summary>
        /// <param name="tokenTypeName">The token type name (e.g., "LogicalAnd").</param>
        /// <returns>A tuple (isFound, result), where isFound is true if the mapping exists, otherwise false.</returns>
        private static (bool, string) GetCSharpOperator(string tokenTypeName)
        {
            var tokenToCSharpOperator = new Dictionary<string, string>
            {
                { "NotEqual", "!=" },              // ≠
                { "GreaterThanOrEqual", ">=" },    // ≥
                { "LessThanOrEqual", "<=" },       // ≤
                { "LogicalAnd", "&&" },            // ∧
                { "LogicalOr", "||" },             // ∨
                { "LogicalNot", "!" },             // ┐
                { "Equal", "==" },
                { "GreaterThan", ">" },
                { "LessThan", "<" },
            };

            if (tokenToCSharpOperator.TryGetValue(tokenTypeName, out var op))
            {
                return (true, op);
            }

            return (false, tokenTypeName);
        }

        /// <summary>
        /// Determines whether the given TokenType is a valid string operator.
        /// </summary>
        private static bool IsStringOperator(TokenType type)
        {
            return type == TokenType.Addition; // for concatenation
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

        // Typecheck by param
        private static bool IsValidExpression(string expression, string type)
        {
            var code = $"class Temp {{ void Test() {{ bool x = {expression}; }} }}";
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(
                "TempCheck",
                new[] { tree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            // Find the initializer expression (e.g., "1 + 2 < 0")
            var varDecl = root.DescendantNodes()
                              .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax>()
                              .FirstOrDefault();

            if (varDecl?.Initializer == null)
            {
                return false;
            }

            var expressionSyntax = varDecl.Initializer.Value;
            var typeInfo = model.GetTypeInfo(expressionSyntax);

            return typeInfo.ConvertedType?.ToDisplayString() == type;
        }

        // Basic (stricter) concept of variable name regex, Cant be longer than 511 character
        [GeneratedRegex(@"\G([a-z_][\p{L}0-9_]{0,510})", RegexOptions.Compiled)]
        private static partial Regex IdentifierRegex();
    }
}
