// <copyright file="IGenerateCode.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator.Interfaces
{
    using Proton.Semantic;

    /// <summary>
    /// Defines the contract for a code generator responsible for transforming validated semantic data
    /// into target code using a specified formatting shell.
    /// </summary>
    public interface IGenerateCode
    {
        /// <summary>
        /// Generates target code using the provided semantic symbol table and formatting shell.
        /// </summary>
        /// <param name="symbolTable">
        /// A <see cref="SymbolTable"/> representing the semantic structure of the program,
        /// including variable definitions and their associated values and types.
        /// </param>
        /// <param name="expressionShell">
        /// A template string used to format the generated code expressions.
        /// </param>
        /// <returns>
        /// A <see cref="GeneratorResult"/> containing the final generated code as a single formatted string.
        /// </returns>
        GeneratorResult Generate(SymbolTable symbolTable, string expressionShell);
    }
}
