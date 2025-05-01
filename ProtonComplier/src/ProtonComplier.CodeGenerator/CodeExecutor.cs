// <copyright file="CodeExecutor.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator
{
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Proton.CodeGenerator.Interfaces;

    /// <summary>
    /// Provides functionality to dynamically compile and execute C# source code in-memory using Roslyn.
    /// </summary>
    public class CodeExecutor : ICodeExecutor
    {
        /// <summary>
        /// Compiles and executes the provided C# code. Captures any compilation or runtime errors.
        /// </summary>
        /// <param name="code">The C# code to compile and execute.</param>
        /// <returns>
        /// A <see cref="GeneratorResult"/> containing the output of the executed code,
        /// or an empty result if compilation or execution fails.
        /// </returns>
        public GeneratorResult ExecuteCode(string code)
        {
            List<string> errors = new ();

            // Parse and compile the source code
            var tree = SyntaxFactory.ParseSyntaxTree(code);

            var compilation = CSharpCompilation.Create(
               "prog.dll",
               options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
               syntaxTrees: new[] { tree },
               references: new[] { MetadataReference.CreateFromFile(typeof(uint).Assembly.Location) });

            // Check for errors
            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any())
            {
                foreach (var diagnostic in diagnostics)
                {
                    var location = diagnostic.Location.GetLineSpan(); // Get line & column info
                    int line = location.StartLinePosition.Line + 1;    // Line number (1-based index)
                    int column = location.StartLinePosition.Character + 1; // Column number (1-based index)

                    string errorMessage = $"Error at Line {line}, Column {column}: {diagnostic.GetMessage()}";
                    errors.Add(errorMessage);
                }

                return new GeneratorResult(code, string.Empty, errors);
            }
            else
            {
                try
                {
                    // Emit assembly in-memory (no DLL is generated on disk)
                    Assembly compiledAssembly;
                    using (var stream = new MemoryStream())
                    {
                        var compileResult = compilation.Emit(stream);
                        if (!compileResult.Success)
                        {
                            errors.Add("Compilation failed.");
                            return new GeneratorResult(code, string.Empty, errors);
                        }

                        compiledAssembly = Assembly.Load(stream.GetBuffer());
                    }

                    // Dynamically call method and print result
                    var calculator = compiledAssembly.GetType("Program");
                    var evaluate = calculator?.GetMethod("Main");

                    if (evaluate != null)
                    {
                        var result = evaluate.Invoke(null, null);
                        return new GeneratorResult(code, (string)result!, errors);
                    }
                    else
                    {
                        errors.Add("Error: No entry method found in compiled code.");
                        return new GeneratorResult(code, string.Empty, errors);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Runtime Error: {ex.Message}");
                    return new GeneratorResult(code, string.Empty, errors);
                }
            }
        }
    }
}
