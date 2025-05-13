global using NUnit.Framework;
using Moq;
using Proton.CodeGenerator;
using Proton.CodeGenerator.Services;
using Proton.ErrorHandler;
using Proton.Lexer;
using Proton.Lexer.Enums;
using Proton.Lexer.Interfaces;
using Proton.Lexer.Services;
using Proton.Parser;
using Proton.Parser.Service;
using Proton.Semantic;
using Proton.Semantic.Interfaces;
using Proton.Semantic.Services;

namespace CodeGeneration.Tests
{
    [TestFixture]
    public class CodeGenTests
    {
        private static string FilesPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        // Expected results per test file
        private static readonly Dictionary<string, (bool IsSuccessful, string Result, int ErrorsCount)>
           ExpectedResults = new()
           {
                { "Ok01.prtn", (true, "Result: 1\r\n", 0) },
                { "Ok02.prtn", (true, "Result: 3\r\n", 0) },
                { "Ok03.prtn", (true, "Result: 4\r\n", 0) },
                { "Ok04.prtn", (true, "Result: 2\r\n", 0) },
                { "Ok05.prtn", (true, "Result: True\r\n", 0) },
                { "Ok06.prtn", (true, "Result: 8\r\n", 0) },
                { "Ok07.prtn", (true, "Result: 23\r\n", 0) },
                { "Ok08.prtn", (true, "Result: 3\r\n", 0) },
                { "Ok09.prtn", (true, "Result: 1\r\nResult: 2\r\nResult: 3\r\n", 0) },
                { "Ok10.prtn",  (true, "Result: 57\r\n", 0) },
                { "Ok11.prtn",  (true, "Result: 90\r\n", 0) },
                { "Ok12.prtn",  (true, "Result: 68\r\n", 0) },
                { "Ok13.prtn",  (true, "Result: 90\r\n", 0) },
                { "Ok14.prtn",  (true, "Result: 90\r\n", 0) },
                { "Ok15.prtn",  (true, "Result: 68,6\r\n", 0) },
                { "Ok16.prtn",  (true, "Result: 57,3\r\n", 0) },
                { "Ok17.prtn",  (true, "Result: 37,3\r\n", 0) },
                { "Ok18.prtn",  (true, "Result: 8\r\nResult: 12\r\n", 0) },
                { "Ok19.prtn",  (true, "Result: 950\r\n", 0) },
           };

        // Returns test case data: each file path
        private static IEnumerable<TestCaseData> GetOkFiles()
        {
            if (!Directory.Exists(FilesPath))
                Assert.Fail($"Test directory '{FilesPath}' does not exist.");

            var files = Directory.GetFiles(FilesPath, "*.prtn");

            if (files.Length == 0)
                Assert.Fail("No .prtn files found in the ErrorFiles directory.");

            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);

                if (!ExpectedResults.TryGetValue(fileName, out var expectations))
                {
                    Assert.Fail($"No expected result defined for file '{fileName}'.");
                }

                yield return new TestCaseData(filePath,
                                              expectations.IsSuccessful,
                                              expectations.Result,
                                              expectations.ErrorsCount);
            }
        }

        [TestCaseSource(nameof(GetOkFiles))]
        public async Task TestCodeGenerationResult(
            string filePath,
            bool isSuccessful,
            string expectedresult,
            int errorsCount)
        {
            string content = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            TestContext.WriteLine($"Testing file: {fileName}");

            GeneratorResult resultCodeGeneration;
            bool success = false;
            int errors = 0;
            string result = null!;

            var tokens = new Tokenizer().Tokenize(content);
            var resultLexicalService = new LexicalService(new Tokenizer(), new LexicalAnalyzer()).Complie(content);
            if (resultLexicalService.isSuccessful)
            {
                var resultParserService = new ParserService(new SyntaxAnalyzer()).Complie(resultLexicalService.sections);
                if (resultParserService.isSuccessful)
                {
                    var resultSemanticService = new SemanticService(new SemanticAnalyzer()).Complie(resultParserService.sections);
                    if (resultSemanticService.isSuccessful)
                    {
                        resultCodeGeneration = await new CodeGeneratorService(new GenerateCode(), new CodeExecutor()).GenerateAndExecute(resultSemanticService.table);
                        success = resultCodeGeneration.isSuccessful;
                        errors = resultCodeGeneration.errors.Count();
                        result = resultCodeGeneration.result;
                    }
                }
            }


            Assert.Multiple(() =>
            {
                Assert.That(success, Is.EqualTo(isSuccessful),
                    isSuccessful
                        ? $"File with name:'{fileName}', unsuccessful code genartion."
                        : $"File with name:'{fileName}', successful code genartion.");

                Assert.That(errors, Is.EqualTo(errorsCount),
                    $"Expected error(s) count:{errorsCount}, but found {errors} number of errors in file '{fileName}'.");

                Assert.That(result, Is.EqualTo(expectedresult),
                    $"Expected result: {expectedresult}, but found result: {result} in file '{fileName}'.");
            });

            TestContext.WriteLine($"File '{fileName}' passed.");
        }
    }
}