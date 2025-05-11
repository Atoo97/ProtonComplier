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

            var refs = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Linq
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location), // System.Collections
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
            };

            var compilation = CSharpCompilation.Create(
               "prog.dll",
               options: new CSharpCompilationOptions(OutputKind.ConsoleApplication),
               syntaxTrees: new[] { tree },
               references: refs);

            // Check for errors
            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                foreach (var diagnostic in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    var location = diagnostic.Location.GetLineSpan();
                    int line = location.StartLinePosition.Line + 1;
                    int column = location.StartLinePosition.Character + 1;

                    string errorMessage = $"Error at Line {line}, Column {column}: {diagnostic.GetMessage()}";
                    errors.Add(errorMessage);
                }

                return new GeneratorResult(code, string.Empty, errors, false);
            }
            else
            {
                try
                {
                    // Emit assembly in-memory (no DLL is generated on disk)
                    using var stream = new MemoryStream();
                    var emitResult = compilation.Emit(stream);

                    if (!emitResult.Success)
                    {
                        errors.Add("Compilation failed.");
                        return new GeneratorResult(code, string.Empty, errors, false);
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    var compiledAssembly = Assembly.Load(stream.ToArray());

                    // Dynamically call method and print result
                    var programType = compiledAssembly.GetTypes().FirstOrDefault(t => t.Name == "Program");
                    var mainMethod = programType?.GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    if (mainMethod != null)
                    {
                        var originalOut = Console.Out;
                        using var sw = new StringWriter();
                        Console.SetOut(sw);

                        try
                        {
                            var parameters = mainMethod.GetParameters().Length == 1
                                ? new object[] { new string[0] }
                                : null;

                            mainMethod.Invoke(null, parameters);
                        }
                        finally
                        {
                            Console.SetOut(originalOut);
                        }

                        return new GeneratorResult(code, sw.ToString(), errors, true);
                    }
                    else
                    {
                        errors.Add("Error: No static Main method found in compiled code.");
                        return new GeneratorResult(code, string.Empty, errors, false);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Runtime Error: {ex.Message}");
                    return new GeneratorResult(code, string.Empty, errors, false);
                }
            }
        }
    }
}
