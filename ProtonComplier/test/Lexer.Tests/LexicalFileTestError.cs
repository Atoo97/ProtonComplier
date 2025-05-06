global using NUnit.Framework;
using Moq;
using Proton.ErrorHandler;
using Proton.Lexer;
using Proton.Lexer.Enums;
using Proton.Lexer.Interfaces;
using Proton.Lexer.Services;

namespace Lexer.Tests
{
    [TestFixture]
    public class LexicalFileTestError
    {
        private static string ErrorFilesPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "LexicalTestFiles", "ErrorFiles");

        // Expected results per test file
        private static readonly Dictionary<string, (int ExpectedErrorCount, int ExpectedWarningCount, List<string> ExpectedErrorIDs, List<string> ExpectedWarningIDs, bool ExpectUnknownToken)>
            ExpectedResults = new ()
            {
                { "error01.prtn", (1, 0, new List<string> { "001" }, new List<string>(), false) },
                { "error02.prtn", (4, 0, new List<string> { "005" }, new List<string>(), false) },
                { "error03.prtn", (3, 1, new List<string> { "005" }, new List<string> { "006" }, false) },
                { "error04.prtn", (4, 0, new List<string> { "005" }, new List<string>(), false) },
                { "error05.prtn", (2, 0, new List<string> { "004", "008" }, new List<string>(), false) },
                { "error06.prtn", (5, 1, new List<string> { "004", "005", "011" }, new List<string> { "006" }, true) },
            };

        private static IEnumerable<TestCaseData> GetErrorFiles()
        {
            if (!Directory.Exists(ErrorFilesPath))
                Assert.Fail($"Test directory '{ErrorFilesPath}' does not exist.");

            var files = Directory.GetFiles(ErrorFilesPath, "*.prtn");

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
                                              expectations.ExpectedErrorCount,
                                              expectations.ExpectedWarningCount,
                                              expectations.ExpectedErrorIDs,
                                              expectations.ExpectedWarningIDs,
                                              expectations.ExpectUnknownToken)
                    .SetName($"ErrorTest_{fileName.Replace(".prtn", "")}");
            }
        }

        [TestCaseSource(nameof(GetErrorFiles))]
        public void TestLexicalAnalyzer_ErrorFile(
            string filePath,
            int expectedErrorCount,
            int expectedWarningCount,
            List<string> expectedErrorIDs,
            List<string> expectedWarningIDs,
            bool expectUnknownToken)
        {
            string content = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            TestContext.WriteLine($"Testing file: {fileName}");

            var tokens = new Tokenizer().Tokenize(content);
            var result = new LexicalService(new Tokenizer(), new LexicalAnalyzer()).Complie(content);

            bool hasUnknownTokens = tokens.Any(t => t.TokenType == TokenType.Unknown);
            var actualErrorIDs = result.errors.Select(e => e.ID).ToList();
            var actualWarningIDs = result.warnings.Select(w => w.ID).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(hasUnknownTokens, Is.EqualTo(expectUnknownToken),
                    expectUnknownToken
                        ? $"Expected unknown token in file '{fileName}', but none found."
                        : $"Unexpected unknown token found in file '{fileName}'.");

                Assert.That(result.errors.Count, Is.EqualTo(expectedErrorCount),
                    $"Expected {expectedErrorCount} error(s), but found {result.errors.Count} in file '{fileName}'.");

                Assert.That(result.warnings.Count, Is.EqualTo(expectedWarningCount),
                    $"Expected {expectedWarningCount} warning(s), but found {result.warnings.Count} in file '{fileName}'.");

                foreach (var expectedError in expectedErrorIDs)
                {
                    Assert.That(actualErrorIDs, Contains.Item(expectedError),
                        $"Missing expected error ID '{expectedError}' in file '{fileName}'.");
                }

                foreach (var expectedWarning in expectedWarningIDs)
                {
                    Assert.That(actualWarningIDs, Contains.Item(expectedWarning),
                        $"Missing expected warning ID '{expectedWarning}' in file '{fileName}'.");
                }
            });

            TestContext.WriteLine($"File '{fileName}' passed.");
        }
    }
}
