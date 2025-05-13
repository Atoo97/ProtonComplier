// <copyright file="GenerateCode.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Microsoft.CodeAnalysis;
    using Proton.CodeGenerator.Interfaces;
    using Proton.Lexer.Enums;
    using Proton.Semantic;

    /// <summary>
    /// Provides the implementation for generating code based on semantic information
    /// and a predefined expression shell template.
    /// </summary>
    public partial class GenerateCode : IGenerateCode
    {
        private static Symbol preconditionSymbol = null!;

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

            var symbols = GroupSymbol(symbolTable, true);
            string mergedCode;

            var statePlaceCode = GenerateState(symbols, indent);
            var inputCode = GenerateInput(symbolTable, symbols, indent);
            var preconditionCode = GeneratePrecondition(preconditionSymbol, indent);

            // Check if the precondition code contains "@"
            if (preconditionCode.Contains('@'))
            {
                // Replace @ in the precondition string with the generated postcondition code
                mergedCode = preconditionCode.Replace("@", GeneratePostcondition(symbolTable, GroupSymbol(symbolTable, false), indent + "    "));
            }
            else
            {
                mergedCode = preconditionCode.Replace(preconditionCode, indent + GeneratePostcondition(symbolTable, symbols, indent));
            }

            return new GeneratorResult(expressionShell.Replace("$", statePlaceCode + "\n" + inputCode + "\n" + mergedCode), null!, null!, true);
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
            sb.AppendLine($"{indent}// Input:");

            // Create a dictionary to group symbols by their serialized value string
            var groupedByValue = symbolTable.Symbols
                .Where(s => s.Name != "0" && s.Name != "0C" && !s.IsResult && s.IsInitialized) // skip precondition
                .GroupBy(s => string.Join(" ", s.Value.Select(t => t.TokenValue)))
                .ToDictionary(g => g.Key, g => g.OrderBy(sym => symbolTable.Symbols
                    .ToList()
                    .FindIndex(s => s.Name == sym.Name)).ToList());

            foreach (var group in groupedByValue.Values)
            {
                if (group.Count == 0)
                {
                    continue;
                }

                var firstSymbol = group.First();
                string variableType = firstSymbol.Type.ToString();  // Get the type of the first symbol
                string csharpType = TypeMapping.ToCSharpType(variableType);
                bool isList = firstSymbol.IsList;
                string value;

                if (isList && firstSymbol.Value.First().TokenType == TokenType.QuestionMarks)
                {
                    value = string.Empty;
                }
                else
                {
                    value = firstSymbol.ValueTokens.ToString().TrimEnd(',');
                }

                string declaration = isList
                    ? $"{string.Join(" = ", group.Select(s => s.Name))} = new {csharpType}[] {{{value}}};"
                    : $"{string.Join(" = ", group.Select(s => s.Name))} = {value};";

                sb.AppendLine($"{indent}{declaration}");  // Add the declaration to the StringBuilder
            }

            return sb.ToString();
        }

        private static string GeneratePrecondition(Symbol symbol, string indent)
        {
            StringBuilder sb = new ();
            sb.AppendLine($"{indent}//Precondition:");

            if (symbol is not null)
            {
                // Generate the output
                // Create the 'if' statement using the symbol's ValueTokens as the condition
                string condition = symbol.ValueTokens.ToString().Trim();

                sb.AppendLine($"{indent}if ({condition})");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    @");
                sb.AppendLine($"{indent}}}");
            }

            return sb.ToString();
        }

        private static string GeneratePostcondition(SymbolTable symbolTable, List<List<Symbol>> symbols, string indent)
        {
            StringBuilder sb = new ();
            sb.AppendLine($"//PostCondition:");

            foreach (var groupitems in symbols)
            {
                // Get the initialized items only from the groupitems
                var group = groupitems
                    .Where(s => s.IsResult)
                    .ToList();

                if (group.Count == 0)
                {
                    continue;
                }

                // Check if group contains a symbol with Name == "0C"
                if (group.Any(s => s.Name == "0C"))
                {
                    var parts = group.First().ValueTokens.ToString().Split('|').ToList();
                    if (parts.Count() > 0 && parts.Last() == "")
                    {
                        parts.RemoveAt(parts.Count - 1);
                    }

                    int count = 0;
                    int cnt = 0;
                    for (var i = 0; i < parts.Count(); i += 2)
                    {
                        sb.AppendLine($"{indent}{parts[i]}");
                        sb.AppendLine($"{indent}" + '{');

                        count += int.Parse(parts[i + 1]);
                        for (var j = cnt; j < count; j++)
                        {
                            var firstSymbol = symbolTable.FindSymbol(group.First().Value[j]) !;
                            string csharpType = TypeMapping.ToCSharpType(firstSymbol.Type.ToString());
                            bool isList = firstSymbol.IsList;
                            string value;

                            if (isList && firstSymbol.Value.First().TokenType == TokenType.QuestionMarks)
                            {
                                value = string.Empty;
                            }
                            else
                            {
                                value = firstSymbol.ValueTokens.ToString().TrimEnd(',');
                            }

                            string declaration = isList
                                ? $"{firstSymbol.Name} = new {csharpType}[] {{{value}}};"
                                : $"{firstSymbol.Name} = {value};";

                            sb.AppendLine($"{indent}    {declaration}");  // Add the declaration to the StringBuilder

                            if (isList)
                            {
                                sb.AppendLine($"{indent}    foreach (var item in {firstSymbol.Name})");
                                sb.AppendLine($"{indent}    {{");
                                sb.AppendLine($"{indent}        Console.WriteLine(\"Result: \" + item);");
                                sb.AppendLine($"{indent}    }}");
                            }
                            else
                            {
                                sb.AppendLine($"{indent}    Console.WriteLine(\"Result: \" + {firstSymbol.Name});");
                            }

                            sb.AppendLine($"{indent}");
                        }

                        cnt += count;

                        if (i + 1 == count)
                        {
                            sb.AppendLine($"{indent}" + "};");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}" + '}');
                        }
                    }

                    sb.AppendLine($"{indent}");
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
                    else
                    {
                        value = firstSymbol.ValueTokens.ToString().TrimEnd(',');
                    }

                    string declaration = isList
                        ? $"{string.Join(" = ", group.Select(s => s.Name))} = new {csharpType}[] {{{value}}};"
                        : $"{string.Join(" = ", group.Select(s => s.Name))} = {value};";

                    sb.AppendLine($"{indent}{declaration}");  // Add the declaration to the StringBuilder

                    if (isList)
                    {
                        sb.AppendLine($"{indent}foreach (var item in {firstSymbol.Name})");
                        sb.AppendLine($"{indent}{{");
                        sb.AppendLine($"{indent}    Console.WriteLine(\"Result: \" + item);");
                        sb.AppendLine($"{indent}}}");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}Console.WriteLine(\"Result: \" + {firstSymbol.Name});");
                    }
                }
                else
                {
                    // Handle symbols with different values individually
                    foreach (var item in group)
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
                        else
                        {
                            value = firstSymbol.ValueTokens.ToString().TrimEnd(',');
                        }

                        string declaration = isList
                            ? $"{item.Name} = new {csharpType}[] {{{value}}};"
                            : $"{item.Name} = {value};";

                        sb.AppendLine($"{indent}{declaration}");  // Add the declaration to the StringBuilder

                        if (isList)
                        {
                            sb.AppendLine($"{indent}foreach (var item in {firstSymbol.Name})");
                            sb.AppendLine($"{indent}{{");
                            sb.AppendLine($"{indent}    Console.WriteLine(\"Result: \" + item);");
                            sb.AppendLine($"{indent}}}");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}Console.WriteLine(\"Result: \" + {firstSymbol.Name});");
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private static List<List<Symbol>> GroupSymbol(SymbolTable symbolTable, bool b)
        {
            List<List<Symbol>> groupedSymbols = new ();
            List<Symbol> remainingSymbols = new ();

            if (b)
            {
                remainingSymbols = symbolTable.Symbols
                .Where(s => s.Name != "0" && s.Name != "0C") // exclude precondition
                .OrderBy(s => s.SymbolLine)
                .ThenBy(s => s.SymbolColumn)
                .ToList();
            }
            else
            {
                remainingSymbols = symbolTable.Symbols
                .Where(s => s.Name != "0") // exclude precondition
                .OrderBy(s => s.SymbolLine)
                .ThenBy(s => s.SymbolColumn)
                .ToList();
            }

            // Extract precondition symbol if it exists
            preconditionSymbol = symbolTable.Symbols.FirstOrDefault(s => s.Name == "0") !;

            while (remainingSymbols.Any())
            {
                Symbol first = remainingSymbols[0];
                List<Symbol> group = new () { first };
                remainingSymbols.RemoveAt(0);

                int line = first.SymbolLine;

                // Collect all adjacent symbols on the same line
                for (int i = 0; i < remainingSymbols.Count;)
                {
                    Symbol current = remainingSymbols[i];
                    Symbol lastInGroup = group[^1];

                    int expectedNextColumn = lastInGroup.SymbolColumn + lastInGroup.Name.Length;

                    if (current.SymbolLine == line &&
                        Math.Abs(current.SymbolColumn - expectedNextColumn) <= 1)
                    {
                        group.Add(current);
                        remainingSymbols.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                groupedSymbols.Add(group);
            }

            return groupedSymbols;
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
