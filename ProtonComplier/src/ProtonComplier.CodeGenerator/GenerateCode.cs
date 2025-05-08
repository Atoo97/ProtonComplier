// <copyright file="GenerateCode.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Proton.CodeGenerator.Interfaces;
    using Proton.Lexer;
    using Proton.Lexer.Enums;
    using Proton.Semantic;

    /// <summary>
    /// Provides the implementation for generating code based on semantic information
    /// and a predefined expression shell template.
    /// </summary>
    public partial class GenerateCode : IGenerateCode
    {
        /// <summary>
        /// Generates target code by processing the provided <see cref="SymbolTable"/> and
        /// inserting generated output into the specified <paramref name="expressionShell"/>.
        /// </summary>
        /// <param name="symbolTable">
        /// The semantic symbol table containing variables, types, and values used in code generation.
        /// </param>
        /// <param name="expressionShell">
        /// A string template containing a placeholder ('$') where the generated code will be inserted.
        /// </param>
        /// <returns>
        /// A <see cref="GeneratorResult"/> containing the final output string with generated content
        /// injected into the template.
        /// </returns>
        public GeneratorResult Generate(SymbolTable symbolTable, string expressionShell)
        {
            var match = MyRegex().Match(expressionShell);
            string indent = match.Success ? match.Groups[1].Value : string.Empty;

            var symbols = GroupSymbol(symbolTable);

            var statePlaceCode = GenerateState(symbols, indent);
            var inputCode = GenerateInput(symbolTable, symbols, indent);

            return new GeneratorResult(expressionShell.Replace("$", statePlaceCode + "\n" + inputCode), null!, null!, true);
        }

        private static string GenerateState(List<List<Symbol>> symbols, string indent)
        {
            StringBuilder sb = new ();
            sb.AppendLine($"//StatePlace:");

            // Generate the output for the grouped symbols
            foreach (var group in symbols)
            {
                var firstSymbol = group.First();
                string variableType = firstSymbol.Type.ToString();  // Get the type of the first symbol
                string csharpType = TypeMapping.ToCSharpType(variableType);
                bool isList = firstSymbol.IsList;

                string declaration = isList
                    ? $"{csharpType}[] {string.Join(", ", group.Select(s => s.Name))};"
                    : $"{csharpType} {string.Join(", ", group.Select(s => s.Name))};";

                sb.AppendLine($"{indent}{declaration}");  // Add the declaration to the StringBuilder
            }

            return sb.ToString();
        }

        private static string GenerateInput(SymbolTable symbolTable, List<List<Symbol>> symbols, string indent)
        {
            StringBuilder sb = new ();
            sb.AppendLine($"{indent}//Input:");

            foreach (var groupitems in symbols)
            {
                // Get the initialized items only from the groupitems
                var group = groupitems
                    .Where(s => s.IsInitialized)
                    .ToList();

                if (group.Count == 0)
                {
                    continue;
                }

                // Chehck if values are same to each group item:
                var firstValue = string.Concat(group.First().Value.Select(t => t.TokenValue));

                bool allSame = group.All(symbol =>
                    string.Concat(symbol.Value.Select(t => t.TokenValue)) == firstValue);

                if (allSame)
                {
                    // All values in this group are the same, use `firstValue` for assignment
                    var firstSymbol = group.First();
                    string variableType = firstSymbol.Type.ToString();  // Get the type of the first symbol
                    string csharpType = TypeMapping.ToCSharpType(variableType);
                    bool isList = firstSymbol.IsList;
                    string value;

                    if (isList && firstSymbol.Value.First().TokenType == TokenType.QuestionMarks)
                    {
                        value = string.Empty;
                    }
                    else if (variableType == "Text")
                    {
                        value = string.Concat(firstSymbol.Value.Select(t =>
                        {
                            var value = t.TokenValue;

                            // Check if the value is a declared variable in the symbol table
                            bool isDeclaredVariable = t.TokenType == TokenType.Identifier;
                            // Check if the token consists only of letters and spaces
                            bool isStringOperator = t.TokenType == TokenType.String;

                            // If it’s a known variable, keep it as-is; otherwise wrap it in quotes
                            return isDeclaredVariable || !isStringOperator ? value : $"\"{value}\"";
                        }));

                        value = value.TrimEnd(';').Replace(";", ",");
                    }
                    else if (variableType == "Character")
                    {
                        var filtered = firstSymbol.Value
                            .Where(t => t.TokenValue != ";") // Remove all semicolons
                            .Select(t => $"'{t.TokenValue}'"); // Wrap each value in single quotes

                        value = string.Join(", ", filtered); // Join with commas
                    }
                    else if (variableType == "Boolean")
                    {
                        // also typeMatch every operator
                        var filtered = firstSymbol.Value
                           .Where(t => t.TokenValue != ";")
                           .Select(t =>
                           {
                               if (IsBooleanOp(t.TokenType))
                               {
                                   // Replace TokenValue with mapped C# operator
                                   return TypeMapping.ToCSharpType(t.TokenType.ToString());
                               }
                               else if (t.TokenType == TokenType.Bool)
                               {
                                   // Lowercase the first letter of the value
                                   return char.ToLower(t.TokenValue[0]) + t.TokenValue.Substring(1);
                               }
                               else if (t.TokenType == TokenType.String)
                               {
                                   // Lowercase the first letter of the value
                                   return $"\"{t.TokenValue}\"";
                               }

                               return t.TokenValue;
                           });

                        value = string.Join(string.Empty, filtered);
                    }
                    else
                    {
                        value = string.Concat(firstSymbol.Value.Select(t => t.TokenValue));
                        value = value.TrimEnd(';').Replace(";", ",");
                    }

                    string declaration = isList
                        ? $"{string.Join(" = ", group.Select(s => s.Name))} = new {csharpType}[] {{{value}}};"
                        : $"{string.Join(" = ", group.Select(s => s.Name))} = {value};";

                    sb.AppendLine($"{indent}{declaration}");  // Add the declaration to the StringBuilder
                }
                else
                {
                    // Handle symbols with different values individually
                    foreach (var item in groupitems)
                    {
                        var firstSymbol = item;
                        string variableType = firstSymbol.Type.ToString();  // Get the type of the first symbol
                        string csharpType = TypeMapping.ToCSharpType(variableType);
                        bool isList = firstSymbol.IsList;
                        string value;

                        if (isList && firstSymbol.Value.First().TokenType == TokenType.QuestionMarks)
                        {
                            value = string.Empty;
                        }
                        else if (variableType == "Text")
                        {
                            value = string.Concat(firstSymbol.Value.Select(t =>
                            {
                                var value = t.TokenValue;

                                // Check if the value is a declared variable in the symbol table
                                bool isDeclaredVariable = t.TokenType == TokenType.Identifier;
                                // Check if the token consists only of letters and spaces
                                bool isStringOperator = t.TokenType == TokenType.String;

                                // If it’s a known variable, keep it as-is; otherwise wrap it in quotes
                                return isDeclaredVariable || !isStringOperator ? value : $"\"{value}\"";
                            }));

                            value = value.TrimEnd(';').Replace(";", ",");
                        }
                        else if (variableType == "Character")
                        {
                            var filtered = firstSymbol.Value
                            .Where(t => t.TokenValue != ";") // Remove all semicolons
                            .Select(t => $"'{t.TokenValue}'"); // Wrap each value in single quotes

                            value = string.Join(", ", filtered); // Join with commas
                        }
                        else if (variableType == "Boolean")
                        {
                            // also typeMatch every operator
                            var filtered = firstSymbol.Value
                               .Where(t => t.TokenValue != ";")
                               .Select(t =>
                               {
                                   if (IsBooleanOp(t.TokenType))
                                   {
                                       // Replace TokenValue with mapped C# operator
                                       return TypeMapping.ToCSharpType(t.TokenType.ToString());
                                   }
                                   else if (t.TokenType == TokenType.Bool)
                                   {
                                       // Lowercase the first letter of the value
                                       return char.ToLower(t.TokenValue[0]) + t.TokenValue.Substring(1);
                                   }
                                   else if (t.TokenType == TokenType.String)
                                   {
                                       // Lowercase the first letter of the value
                                       return $"\"{t.TokenValue}\"";
                                   }

                                   return t.TokenValue;
                               });

                            value = string.Join(string.Empty, filtered);
                        }
                        else
                        {
                            value = string.Concat(firstSymbol.Value.Select(t => t.TokenValue));
                            value = value.TrimEnd(';').Replace(";", ",");
                        }

                        string declaration = isList
                        ? $"{item.Name} = new {csharpType}[] {{{value}}};"
                        : $"{item.Name} = {value};";

                        sb.AppendLine($"{indent}{declaration}");  // Add the declaration to the StringBuilder
                    }
                }
            }

            return sb.ToString();
        }

        private static List<List<Symbol>> GroupSymbol(SymbolTable symbolTable)
        {
            List<List<Symbol>> symbols = new ();
            List<Symbol> currentGroup = new ();

            foreach (Symbol symbol in symbolTable.Symbols)
            {
                if (currentGroup.Count == 0)
                {
                    currentGroup.Add(symbol);
                    continue;
                }
                else
                {
                    // Check if the current symbol is on the same line and if the name of the current symbol is +1 from the last one
                    int lastEndColumn = currentGroup[^1].SymbolColumn + currentGroup[^1].Name.Length;
                    if (currentGroup[^1].SymbolLine == symbol.SymbolLine &&
                        Math.Abs(lastEndColumn - symbol.SymbolColumn) <= 1) // allow small offset
                    {
                        currentGroup.Add(symbol);
                    }
                    else
                    {
                        symbols.Add(new List<Symbol>(currentGroup));  // Add the current group to symbols
                        currentGroup.Clear();
                        currentGroup.Add(symbol);
                    }
                }
            }

            // Add the last group to symbols if it exists
            if (currentGroup.Count > 0)
            {
                symbols.Add(currentGroup);
            }

            return symbols;
        }

        /// <summary>
        /// Determines whether the given TokenType is a valid boolean operator.
        /// </summary>
        private static bool IsBooleanOp(TokenType type)
        {
            return type is TokenType.NotEqual or
                   TokenType.GreaterThanOrEqual or
                   TokenType.LessThanOrEqual or
                   TokenType.LogicalAnd or
                   TokenType.LogicalOr or
                   TokenType.LogicalNot;
        }

        [GeneratedRegex(@"^(\s*)\$\s*$", RegexOptions.Multiline)]
        private static partial Regex MyRegex();
    }
}
