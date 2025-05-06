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
    public class LexicalFileTestOK
    {
        private static string OkFilesPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "LexicalTestFiles", "OkFiles");

        // Returns test case data: each file path
        private static IEnumerable<TestCaseData> GetOkFiles()
        {
            if (!Directory.Exists(OkFilesPath))
                Assert.Fail($"Test directory '{OkFilesPath}' does not exist.");

            var files = Directory.GetFiles(OkFilesPath, "*.prtn");

            if (files.Length == 0)
                Assert.Fail("No .prtn files found in the test directory.");

            foreach (var file in files)
            {
                yield return new TestCaseData(file).SetName(Path.GetFileName(file));
            }
        }

        [TestCaseSource(nameof(GetOkFiles))]
        public void TestLexicalAnalyzer_SingleFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            TestContext.WriteLine($"Testing file: {fileName}");

            // Act
            var result = new LexicalService(new Tokenizer(), new LexicalAnalyzer()).Complie(content);

            // Assertions
            var allTokens = result.sections.SelectMany(kvp => kvp.Value).ToList();                   // Flatten all tokens from all sections
            bool hasUnknownTokens = allTokens.Any(t => t.TokenType == TokenType.Unknown);
            string[] expectedSections = ["StateSpace", "Input", "Precondition", "Postcondition"];
            Assert.Multiple(() =>
            {
                Assert.That(hasUnknownTokens, Is.False, $"File '{fileName}' contains unknown tokens.");
                Assert.That(result.isSuccessful, Is.True, $"File '{fileName}' contains errors or warnings.");

                // Check that all expected macro sections exist
                foreach (var section in expectedSections)
                {
                    Assert.That(result.sections.ContainsKey(section), Is.True,
                        $"Expected section '{section}' not found in file '{fileName}'.");
                }

                // Check that only the expected 4 sections are present
                Assert.That(result.sections.Keys, Is.EquivalentTo(expectedSections),
                    $"File '{fileName}' contains unexpected macro sections.");
            });

            TestContext.WriteLine($"File: {fileName} test successful.");
        }
    }
}
