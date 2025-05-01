// <copyright file="CodeGeneratorService.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator.Services
{
    using Proton.CodeGenerator.Interfaces;
    using Proton.Semantic;

    /// <summary>
    /// Provides a service for generating and executing code based on a symbol table and a code template.
    /// It utilizes the code generator to create code and the code executor to compile and run the generated code.
    /// </summary>
    public class CodeGeneratorService(IGenerateCode codeGenerator, ICodeExecutor codeExecutor)
    {
        private readonly IGenerateCode codeGenerator = codeGenerator;
        private readonly ICodeExecutor codeExecutor = codeExecutor;
        private readonly string path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "codestructure.txt");

        /// <summary>
        /// Generates code based on the provided symbol table, executes it, and returns the result.
        /// </summary>
        /// <param name="symbolTable">The symbol table containing the parsed elements used for generating the code.</param>
        /// <returns>
        /// A <see cref="GeneratorResult"/> containing the output of the executed code.
        /// If any errors occur during generation or execution, they will be reflected in the result.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the default code template file is not found at the expected path.
        /// </exception>
        public async Task<GeneratorResult> GenerateAndExecute(SymbolTable symbolTable)
        {
            if (!System.IO.File.Exists(this.path))
            {
                throw new FileNotFoundException("Default code template file not found.", this.path);
            }

            var expressionShell = await System.IO.File.ReadAllTextAsync(this.path);
            var code = this.codeGenerator.Generate(symbolTable, expressionShell);
            return this.codeExecutor.ExecuteCode(code.code);
        }
    }
}
