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
    public class LexicalTests
    {
        private Mock<ITokenizer>? mockTokenizer;
        private Mock<ILexicalAnalyzer>? mockAnalyzer;

        [SetUp]
        public void Setup()
        {
            mockTokenizer = new Mock<ITokenizer>();
            mockAnalyzer = new Mock<ILexicalAnalyzer>();
        }

        [Test]
        public void LexicalServiceMockTest_Success()
        {
            string code = "#StatePlace\n-23.4";
            var tokens = new List<Token>
            {
                new() { TokenType = TokenType.Macro, TokenValue = "StatePlace", TokenLine = 1, TokenColumn = 1 },
                new() { TokenType = TokenType.Double, TokenValue = "-23.4", TokenLine = 2, TokenColumn = 1 }
            };
            var expectedResult = new LexicalResult
            {
                isSuccessful = true,
                sections = new Dictionary<string, List<Token>> { { "StatePlace", tokens } },
                errors = [],
                warnings = []
            };

            mockTokenizer!.Setup(t => t.Tokenize(code)).Returns(tokens);
            mockAnalyzer!.Setup(a => a.Analyze(tokens)).Returns(expectedResult);
            var service = new LexicalService(mockTokenizer.Object, mockAnalyzer.Object);

            // Act
            var result = service.Complie(code);

            // Assert
            mockTokenizer.Verify(t => t.Tokenize(code), Times.Once);
            mockAnalyzer.Verify(a => a.Analyze(tokens), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(result.isSuccessful, Is.True);
                Assert.That(result.sections.ContainsKey("StatePlace"));
                Assert.That(result.sections["StatePlace"], Has.Count.EqualTo(2));

                // Check if a TokenType.Double exists on line 2
                var hasDoubleToken = result.sections["StatePlace"].Any(t => t.TokenType == TokenType.Double && t.TokenLine == 2);
                Assert.That(hasDoubleToken, Is.True, "Expected a TokenType.Double on line 2.");
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.errors, Is.Empty);
                Assert.That(result.warnings, Is.Empty);
            });
        }

        [Test]
        public void LexicalServiceMockTest_Fail()
        {
            // Arrange
            string code = "#Inpu";
            var tokens = new List<Token>();

            mockTokenizer!.Setup(t => t.Tokenize(code)).Returns(tokens);

            // Simulate the analyzer throwing an exception for invalid tokens
            mockAnalyzer!.Setup(a => a.Analyze(tokens))
                         .Throws(new BaseException("P000","Invalid macro definition at line 1"));

            var service = new LexicalService(mockTokenizer.Object, mockAnalyzer.Object);

            // Act & Assert
            var ex = Assert.Throws<BaseException>(() => service.Complie(code));
            Assert.That(ex.Message, Does.Contain("Invalid macro definition at line 1"));
        }

        [Test]
        public void TokenizerTest1()
        {
            var input = "#StatePlace\r\n_var1:N;\r\n_var2:R;\r";
            var tokens = new Tokenizer().Tokenize(input);

            var expectedTokens = new List<(TokenType, string)>
            {
                (TokenType.Macro, "StatePlace"),
                (TokenType.Newline, "\n"),
                (TokenType.Identifier, "_var1"),
                (TokenType.Colon, ":"),
                (TokenType.Natural, "N"),
                (TokenType.Semicolon, ";"),
                (TokenType.Newline, "\n"),
                (TokenType.Identifier, "_var2"),
                (TokenType.Colon, ":"),
                (TokenType.Real, "R"),
                (TokenType.Semicolon, ";"),
                (TokenType.Newline, "\n"),
            };

            Assert.That(tokens, Has.Count.EqualTo(expectedTokens.Count));

            for (int i = 0; i < expectedTokens.Count; i++)
            {
                Assert.Multiple(() =>
                {
                    // Console.WriteLine(tokens[i].ToString());
                    Assert.That(tokens[i].TokenType, Is.EqualTo(expectedTokens[i].Item1));
                    Assert.That(tokens[i].TokenValue, Is.EqualTo(expectedTokens[i].Item2));
                });
            }
        }

        [Test]
        public void TokenizerTest2()
        {
            string input = "@N;R[]\r";
            var tokens = new Tokenizer().Tokenize(input);

            var expectedTokens = new List<Token>
            {
                new() { TokenType = TokenType.Unknown, TokenValue = "@", TokenLine = 1, TokenColumn = 1},
                new() { TokenType = TokenType.Natural, TokenValue = "N", TokenLine = 1, TokenColumn = 2},
                new() { TokenType = TokenType.Semicolon, TokenValue = ";", TokenLine = 1, TokenColumn = 3},
                new() { TokenType = TokenType.Real, TokenValue = "R", TokenLine = 1, TokenColumn = 4},
                new() { TokenType = TokenType.ListSpecifier, TokenValue = "[]", TokenLine = 1, TokenColumn = 5}
            };

            // Ensure no unknown tokens exist
            bool hasUnknownTokens = tokens.Any(t => t.TokenType == TokenType.Unknown);
            Assert.That(hasUnknownTokens, Is.True, $"Input: '{input}' not contains unknown tokens.");
            // TestContext.WriteLine($"    Input: '{input}' contains unknown tokens.");

            // Ensure all expected tokens are present
            foreach (var expected in expectedTokens)
            {
                Assert.That(tokens, Has.Exactly(1).Matches<Token>(t =>
                    t.TokenType == expected.TokenType &&
                    t.TokenValue == expected.TokenValue &&
                    t.TokenLine == expected.TokenLine &&
                    t.TokenColumn == expected.TokenColumn),
                    $"Expected token not found: {expected.TokenType} ; {expected.TokenValue} at Line: {expected.TokenLine}, Column: {expected.TokenColumn}");
            }
        }


        [Test]
        public void TokenizerTest3()
        {
            string input = "-0.12--234.5==0.12\r";
            var tokens = new Tokenizer().Tokenize(input);

            var expectedTokens = new List<Token>
            {
                new() { TokenType = TokenType.Double, TokenValue = "-0.12", TokenLine = 1, TokenColumn = 1},
                new() { TokenType = TokenType.Subtraction, TokenValue = "-", TokenLine = 1, TokenColumn = 6 },
                new() { TokenType = TokenType.Double, TokenValue = "-234.5", TokenLine = 1, TokenColumn = 7 },
                new() { TokenType = TokenType.Equal, TokenValue = "==", TokenLine = 1, TokenColumn = 13 },
                new() { TokenType = TokenType.Double, TokenValue = "0.12", TokenLine = 1, TokenColumn = 15 }
            };

            // Ensure no unknown tokens exist
            bool hasUnknownTokens = tokens.Any(t => t.TokenType == TokenType.Unknown);
            Assert.That(hasUnknownTokens, Is.False, $"Input: '{input}' contains unknown tokens.");

            // Ensure all expected tokens are present
            foreach (var expected in expectedTokens)
            {
                Assert.That(tokens, Has.Exactly(1).Matches<Token>(t =>
                    t.TokenType == expected.TokenType &&
                    t.TokenValue == expected.TokenValue &&
                    t.TokenLine == expected.TokenLine &&
                    t.TokenColumn == expected.TokenColumn),
                    $"Expected token not found: {expected.TokenType} - {expected.TokenValue} at Line: {expected.TokenLine}, Column: {expected.TokenColumn}");
            }
        }

        [Test]
        public void TokenizerTest4()
        {
            string input = "_variable:N = -0.23\r";
            var tokens = new Tokenizer().Tokenize(input);

            var expectedTokens = new List<Token>
            {
                new() { TokenType = TokenType.Identifier, TokenValue = "_variable", TokenLine = 1, TokenColumn = 1},
                new() { TokenType = TokenType.Colon, TokenValue = ":", TokenLine = 1, TokenColumn = 10},
                new() { TokenType = TokenType.Natural, TokenValue = "N", TokenLine = 1, TokenColumn = 11},
                new() { TokenType = TokenType.Assign, TokenValue = "=", TokenLine = 1, TokenColumn = 13 },
                new() { TokenType = TokenType.Double, TokenValue = "-0.23", TokenLine = 1, TokenColumn = 15 }
            };

            // Ensure no unknown tokens exist
            bool hasUnknownTokens = tokens.Any(t => t.TokenType == TokenType.Unknown);
            Assert.That(hasUnknownTokens, Is.False, $"Input: '{input}' contains unknown tokens.");

            // Ensure all expected tokens are present
            foreach (var expected in expectedTokens)
            {
                Assert.That(tokens, Has.Exactly(1).Matches<Token>(t =>
                    t.TokenType == expected.TokenType &&
                    t.TokenValue == expected.TokenValue &&
                    t.TokenLine == expected.TokenLine &&
                    t.TokenColumn == expected.TokenColumn),
                    $"Expected token not found: {expected.TokenType} ; {expected.TokenValue} at Line: {expected.TokenLine}, Column: {expected.TokenColumn}");
            }
        }

        [Test]
        public void TokenizerTest5()
        {
            string input = "#StatePlace\r\n_var1:N;\r\n//commentline\r\n_var2:R;\r\n\r\n-123.34, 0.56;\r";
            var tokens = new Tokenizer().Tokenize(input);

            Assert.That(tokens, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                Assert.That(tokens.Exists(t => t.TokenType == TokenType.Macro && t.TokenValue == "StatePlace"), Is.True);
                Assert.That(tokens.Exists(t => t.TokenType == TokenType.Identifier && t.TokenValue == "_var1"), Is.True);
                Assert.That(tokens.Exists(t => t.TokenType == TokenType.Natural), Is.True);
                Assert.That(tokens.Exists(t => t.TokenType == TokenType.Comment), Is.True);
                Assert.That(tokens.Exists(t => t.TokenType == TokenType.Identifier && t.TokenValue == "_var2"), Is.True);
                Assert.That(tokens.Exists(t => t.TokenType == TokenType.Real), Is.True);
            });
        }
    }
}