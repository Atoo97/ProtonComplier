// <copyright file="GenerateCode.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator
{
    using System.Text;
    using System.Text.RegularExpressions;
    using Proton.CodeGenerator.Interfaces;
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

            var statePlaceCode = GenerateStateOrInput(symbolTable, indent, isInput: false);
            var inputCode = string.Empty; // GenerateStateOrInput(symbolTable, indent, isInput: true);

            return new GeneratorResult(expressionShell.Replace("$", statePlaceCode + "\n" + inputCode), null!, null!, true);
        }

        private static string GenerateStateOrInput(SymbolTable symbolTable, string indent, bool isInput)
        {
            StringBuilder sb = new ();
            sb.AppendLine($"//{(isInput ? "Inputs" : "StatePlace")}:\n");

            var groupedByLine = symbolTable.Symbols
                .GroupBy(s => s.SymbolLine)
                .OrderBy(g => g.Key);

            foreach (var group in groupedByLine)
            {
                var first = group.First();
                string variableType = first.Type.ToString();
                string csharpType = TypeMapping.ToCSharpType(variableType);
                bool isList = first.IsList;

                if (!isInput)
                {
                    string declaration = isList
                        ? $"{csharpType}[] {string.Join(", ", group.Select(s => s.Name))};"
                        : $"{csharpType} {string.Join(", ", group.Select(s => s.Name))};";

                    sb.AppendLine($"{indent}{declaration}");
                }
                else
                {
                    if (isList)
                    {
                        foreach (var symbol in group)
                        {
                            var values = string.Join(", ", symbol.Value.Select(t => t.TokenValue));
                            sb.AppendLine($"{indent}{symbol.Name} = new {csharpType}[] {{{values}}};");
                        }
                    }
                    else
                    {
                        var symbolValueMap = group.ToDictionary(
                            s => s,
                            s => s.Value.FirstOrDefault()?.TokenValue ?? "0");

                        var groupedByValue = symbolValueMap
                            .GroupBy(kvp => kvp.Value)
                            .OrderBy(g => g.Key);

                        foreach (var groupVal in groupedByValue)
                        {
                            var names = string.Join(" = ", groupVal.Select(g => g.Key.Name));
                            sb.AppendLine($"{indent}{names} = {groupVal.Key};");
                        }
                    }
                }
            }

            return sb.ToString();
        }

        [GeneratedRegex(@"^(\s*)\$\s*$", RegexOptions.Multiline)]
        private static partial Regex MyRegex();
    }
}
