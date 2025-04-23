global using NUnit.Framework;
using Moq;
using Proton.Lexer;
using Proton.Lexer.Interfaces;
using Proton.Lexer.Services;

namespace TestProject1
{
    [TestFixture]
    public class Tests
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
        public void LexicalService_CompileCall()
        {
            // Arrange
            string code = "#StatePlace\n_var1:N;\n_var2:R";

            var tokens = new List<Token>
            {
                new() { TokenType = TokenType.Macro, TokenValue = "StatePlace", TokenLine = 1, TokenColumn = 1 }
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
            var result = service.Compile(code);

            // Assert
            mockTokenizer.Verify(t => t.Tokenize(code), Times.Once);
            mockAnalyzer.Verify(a => a.Analyze(tokens), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(result.isSuccessful, Is.True);
                Assert.That(result.sections.ContainsKey("StatePlace"));
                Assert.That(result.sections["StatePlace"].Count, Is.EqualTo(1));
            });

            Assert.That(result.errors, Is.Empty);
            Assert.That(result.warnings, Is.Empty);

        }

        [Test]
        public void LexicalService_CompileFail()
        {
            // Arrange
            string code = "#StateP";
            var tokens = new List<Token>();

            mockTokenizer!.Setup(t => t.Tokenize(code)).Returns(tokens);
            mockAnalyzer!.Setup(a => a.Analyze(It.IsAny<List<Token>>()))
                        .Throws(new Exception("Invalid symbol"));

            var service = new LexicalService(mockTokenizer.Object, mockAnalyzer.Object);

            // Act & Assert
            Assert.Throws<Exception>(() => service.Compile(code));
        }

        [Test]
        public void LexicalService_CompileFail2()
        {
            // Arrange
            string code = "#StateP";
            var tokens = new List<Token>();

            mockTokenizer!.Setup(t => t.Tokenize(code)).Returns(tokens);
            mockAnalyzer!.Setup(a => a.Analyze(It.IsAny<List<Token>>()));

            var service = new LexicalService(mockTokenizer.Object, mockAnalyzer.Object);

            // Act
            var result = service.Compile(code);
            Assert.That(result.errors, Is.Not.Empty);
        }

        [Test]
        public void Tokenize_Should_Return_Exact_Token_Sequence()
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

            Assert.That(tokens.Count, Is.EqualTo(expectedTokens.Count));

            for (int i = 0; i < expectedTokens.Count; i++)
            {
                // Console.WriteLine(tokens[i].ToString());
                Assert.That(tokens[i].TokenType, Is.EqualTo(expectedTokens[i].Item1));
                Assert.That(tokens[i].TokenValue, Is.EqualTo(expectedTokens[i].Item2));
            }
        }

        [Test]
        public void Tokenize_Should_Return_Correct_Tokens_From_File()
        {
            var filePath = "TestFiles/LexicalTestFiles/ok.prtn";
            var input = File.ReadAllText(filePath) + "\r";
            var tokens = new Tokenizer().Tokenize(input);

            Assert.IsNotEmpty(tokens);
            Assert.IsTrue(tokens.Exists(t => t.TokenType == TokenType.Macro && t.TokenValue == "StatePlace"));
            Assert.IsTrue(tokens.Exists(t => t.TokenType == TokenType.Identifier && t.TokenValue == "_var1"));
            Assert.IsTrue(tokens.Exists(t => t.TokenType == TokenType.Natural));
            Assert.IsTrue(tokens.Exists(t => t.TokenType == TokenType.Identifier && t.TokenValue == "_var2"));
            Assert.IsTrue(tokens.Exists(t => t.TokenType == TokenType.Real));
        }

    }
}