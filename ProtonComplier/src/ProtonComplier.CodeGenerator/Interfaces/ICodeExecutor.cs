// <copyright file="ICodeExecutor.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator.Interfaces
{
    using Proton.Semantic;

    /// <summary>
    /// Defines the contract for executing generated code and capturing execution results or errors.
    /// </summary>
    public interface ICodeExecutor
    {
        /// <summary>
        /// Executes the provided code string and collects any runtime errors that occur during execution.
        /// </summary>
        /// <param name="code">The generated code to be executed.</param>
        /// <returns>
        /// A <see cref="GeneratorResult"/> representing the output produced by executing the code,
        /// or an empty result if execution fails.
        /// </returns>
        GeneratorResult ExecuteCode(string code);
    }

}
