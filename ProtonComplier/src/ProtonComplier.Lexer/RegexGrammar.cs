// <copyright file="RegexGrammar.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides a collection of regex patterns used to tokenize the input.
    /// Each token type is matched using a predefined regular expression.
    /// </summary>
    public static partial class RegexGrammar
    {
        /// <summary>
        /// Returns a list of token definitions using precompiled regex via the GeneratedRegex attribute.
        /// These token definitions help to tokenize input based on regular expressions.
        /// </summary>
        /// <returns>List of TokenDefinition.</returns>
        public static List<TokenDefinition> GetTokenDefinitions()
        {
            return
            [

                // Higher Level Tokens
                new (TokenType.Comment, CommentRegex()),

                // Keyword Tokens
                new (TokenType.Options, OptionsRegex()),
                new (TokenType.Min, MinRegex()),
                new (TokenType.Max, MaxRegex()),
                new (TokenType.Length, LengthRegex()),

                // Identifier Tokens
                new (TokenType.Natural, NatIdentRegex()),
                new (TokenType.Integer, IntIdentRegex()),
                new (TokenType.Real, RealIdentRegex()),
                new (TokenType.Character, CharIdentRegex()),
                new (TokenType.Text, StrIdentRegex()),
                new (TokenType.Boolean, BoolIdentRegex()),

                // Literal Tokens
                new (TokenType.Double, DoubleRegex()),
                new (TokenType.Uint, UIntegerRegex()),
                new (TokenType.Int, IntegerRegex()),

                new (TokenType.Identifier, IdentifierRegex()), // Identifier token

                new (TokenType.Char, CharacterRegex()),
                new (TokenType.String, StringRegex()),
                new (TokenType.Bool, BooleanRegex()),

                // Operator Tokens
                new (TokenType.Addition, AdditionRegex()),
                new (TokenType.Multiplication, MultiplicationRegex()),
                new (TokenType.Division, DivisionRegex()),
                new (TokenType.Modulus, ModulusRegex()),
                new (TokenType.Equal, EqualRegex()),
                new (TokenType.Assign, AssignRegex()),
                new (TokenType.NotEqual, NotEqualRegex()),
                new (TokenType.GreaterThan, GreaterThanRegex()),
                new (TokenType.LessThan, LessThanRegex()),
                new (TokenType.GreaterThanOrEqual, GreaterThanOrEqualRegex()),
                new (TokenType.LessThanOrEqual, LessThanOrEqualRegex()),
                new (TokenType.LogicalAnd, LogicalAndRegex()),
                new (TokenType.LogicalOr, LogicalOrRegex()),
                new (TokenType.LogicalNot, LogicalNotRegex()),
                new (TokenType.Implication, ImplicationRegex()),
                new (TokenType.UniversalQuant, UniversalQuantRegex()),
                new (TokenType.ExistentialQuant, ExistentialQuantRegex()),
                new (TokenType.Product, ProductRegex()),
                new (TokenType.Summation, SummationRegex()),

                // Punctuator Tokens
                new (TokenType.Semicolon, SemicolonRegex()),
                new (TokenType.Colon, ColonRegex()),
                new (TokenType.Subtraction, SubtractionRegex()),
                new (TokenType.Comma, CommaRegex()),
                new (TokenType.OpenParen, OpenParenRegex()),
                new (TokenType.CloseParen, CloseParenRegex()),
                new (TokenType.OpenBrace, OpenBraceRegex()),
                new (TokenType.CloseBrace, CloseBraceRegex()),
                new (TokenType.ListSpecifier, ListSpecifierRegex()),
                new (TokenType.OpenSqrBrace, OpenSqrBraceRegex()),
                new (TokenType.CloseSqrBrace, CloseSqrBraceRegex()),

                // Special Tokens
                new (TokenType.Macro, MacroRegex()),
                new (TokenType.QuestionMarks, QuestionMarksRegex()),
                new (TokenType.Period, PeriodRegex()),
                new (TokenType.Whitespace, WhitespaceRegex()),

                // new (TokenType.EndOfInput, EndOfInputRegex()),
                // new (TokenType.Unknown, UnknownRegex())
            ];
        }

        // Identifier Regex:
        [GeneratedRegex(@"\G(N)(?![a-zA-Z_0-9])", RegexOptions.Compiled)] // ensure it captures N specifically
        private static partial Regex NatIdentRegex();

        [GeneratedRegex(@"\G(Z)(?![a-zA-Z_0-9])", RegexOptions.Compiled)]
        private static partial Regex IntIdentRegex();

        [GeneratedRegex(@"\G(R)(?![a-zA-Z_0-9])", RegexOptions.Compiled)]
        private static partial Regex RealIdentRegex();

        [GeneratedRegex(@"\G(C)(?![a-zA-Z_0-9])", RegexOptions.Compiled)]
        private static partial Regex CharIdentRegex();

        [GeneratedRegex("\\G(\\$)", RegexOptions.Compiled)]
        private static partial Regex StrIdentRegex();

        [GeneratedRegex(@"\G(B)(?![a-zA-Z_0-9])", RegexOptions.Compiled)]
        private static partial Regex BoolIdentRegex();

        // Basic (stricter) concept of variable name regex: [GeneratedRegex(@"\G([a-z_][\p{L}0-9_]*)", RegexOptions.Compiled)]
        [GeneratedRegex(@"\G([\p{L}\p{N}_][\p{L}\p{N}_]*)", RegexOptions.Compiled)]
        private static partial Regex IdentifierRegex();

        // Literal Regex:
        [GeneratedRegex("\\G(0|[1-9][0-9]*)", RegexOptions.Compiled)]
        private static partial Regex UIntegerRegex(); // Matches unsigned integers (0, 1, 2, ...)

        [GeneratedRegex("\\G(-?(?!-)(?<!\\d)(0|[1-9][0-9]*))", RegexOptions.Compiled)]
        private static partial Regex IntegerRegex();  // Matches integers, ensuring the minus sign is valid

        [GeneratedRegex("\\G(-?(?!-)(?<!\\d)(0|[1-9][0-9]*)\\.[0-9]+)", RegexOptions.Compiled)]
        private static partial Regex DoubleRegex(); // Matches floating-point numbers, allowing a valid negative sign

        [GeneratedRegex("\\G'([^'])'", RegexOptions.Compiled)]
        private static partial Regex CharacterRegex();

        [GeneratedRegex("\\G(\".*?\")", RegexOptions.Compiled)]
        private static partial Regex StringRegex();

        [GeneratedRegex("\\G(True|False)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        private static partial Regex BooleanRegex();

        // Keyword Regex:
        [GeneratedRegex("\\G(Opt)(?![\\p{L}\\p{N}_])", RegexOptions.Compiled)]
        private static partial Regex OptionsRegex();

        [GeneratedRegex(@"\G(Min)(?![a-zA-Z_0-9])", RegexOptions.Compiled)]
        private static partial Regex MinRegex();

        [GeneratedRegex(@"\G(Max)(?![a-zA-Z_0-9])", RegexOptions.Compiled)]
        private static partial Regex MaxRegex();

        [GeneratedRegex("\\G(Length)(?![\\p{L}\\p{N}_])", RegexOptions.Compiled)]
        private static partial Regex LengthRegex();

        // Operator Regex:
        [GeneratedRegex("\\G(\\+)", RegexOptions.Compiled)]
        private static partial Regex AdditionRegex();

        [GeneratedRegex("\\G(-)", RegexOptions.Compiled)]
        private static partial Regex SubtractionRegex();

        [GeneratedRegex("\\G(\\*)", RegexOptions.Compiled)]
        private static partial Regex MultiplicationRegex();

        [GeneratedRegex("\\G(/)", RegexOptions.Compiled)]
        private static partial Regex DivisionRegex();

        [GeneratedRegex("\\G(%)", RegexOptions.Compiled)]
        private static partial Regex ModulusRegex();

        [GeneratedRegex("\\G(=)", RegexOptions.Compiled)]
        private static partial Regex AssignRegex();

        [GeneratedRegex("\\G(==)", RegexOptions.Compiled)]
        private static partial Regex EqualRegex();

        [GeneratedRegex("\\G(≠)", RegexOptions.Compiled)]
        private static partial Regex NotEqualRegex();

        [GeneratedRegex("\\G(>)", RegexOptions.Compiled)]
        private static partial Regex GreaterThanRegex();

        [GeneratedRegex("\\G(<)", RegexOptions.Compiled)]
        private static partial Regex LessThanRegex();

        [GeneratedRegex("\\G(≥)", RegexOptions.Compiled)]
        private static partial Regex GreaterThanOrEqualRegex();

        [GeneratedRegex("\\G(≤)", RegexOptions.Compiled)]
        private static partial Regex LessThanOrEqualRegex();

        [GeneratedRegex("\\G(∧)", RegexOptions.Compiled)]
        private static partial Regex LogicalAndRegex();

        [GeneratedRegex("\\G(∨)", RegexOptions.Compiled)]
        private static partial Regex LogicalOrRegex();

        [GeneratedRegex("\\G(┐)", RegexOptions.Compiled)]
        private static partial Regex LogicalNotRegex();

        [GeneratedRegex("\\G(→)", RegexOptions.Compiled)]
        private static partial Regex ImplicationRegex();

        [GeneratedRegex("\\G(∀)", RegexOptions.Compiled)]
        private static partial Regex UniversalQuantRegex();

        [GeneratedRegex("\\G(∃)", RegexOptions.Compiled)]
        private static partial Regex ExistentialQuantRegex();

        [GeneratedRegex("\\G(∏)", RegexOptions.Compiled)]
        private static partial Regex ProductRegex();

        [GeneratedRegex("\\G(∑)", RegexOptions.Compiled)]
        private static partial Regex SummationRegex();

        // Punctuator Regex:
        [GeneratedRegex("\\G(;)", RegexOptions.Compiled)]
        private static partial Regex SemicolonRegex();

        [GeneratedRegex("\\G(:)", RegexOptions.Compiled)]
        private static partial Regex ColonRegex();

        [GeneratedRegex("\\G(,)", RegexOptions.Compiled)]
        private static partial Regex CommaRegex();

        [GeneratedRegex("\\G(\\()", RegexOptions.Compiled)]
        private static partial Regex OpenParenRegex();

        [GeneratedRegex("\\G(\\))", RegexOptions.Compiled)]
        private static partial Regex CloseParenRegex();

        [GeneratedRegex("\\G(\\{)", RegexOptions.Compiled)]
        private static partial Regex OpenBraceRegex();

        [GeneratedRegex("\\G(\\})", RegexOptions.Compiled)]
        private static partial Regex CloseBraceRegex();

        [GeneratedRegex("\\G(\\[])", RegexOptions.Compiled)]
        private static partial Regex ListSpecifierRegex();

        [GeneratedRegex("\\G(\\[)", RegexOptions.Compiled)]
        private static partial Regex OpenSqrBraceRegex();

        [GeneratedRegex("\\G(\\])", RegexOptions.Compiled)]
        private static partial Regex CloseSqrBraceRegex();

        // Special Regex:
        [GeneratedRegex(@"\G(#([^\r\n]*))", RegexOptions.Compiled)]
        private static partial Regex MacroRegex();

        [GeneratedRegex("\\G(\\?\\?)", RegexOptions.Compiled)]
        private static partial Regex QuestionMarksRegex();

        [GeneratedRegex("\\G(\\.)", RegexOptions.Compiled)]
        private static partial Regex PeriodRegex();

        [GeneratedRegex(@"\G([ \t\r\n]+)", RegexOptions.Compiled)]
        private static partial Regex WhitespaceRegex();

        [GeneratedRegex(@"\G(//[^\r\n]*)", RegexOptions.Compiled)]
        private static partial Regex CommentRegex();
    }
}

/*
 General rules for matching:
    1) Longest match principle:     Form a token from the longest matching string of characters.
    2) Priority principle:          If the longest matching string can match multiple regular expressions, the one that is earlier in the order “wins”


 Why Use[GeneratedRegex]?
    Performance Boost:
        The regex patterns are compiled at compile-time instead of runtime, avoiding overhead from RegexOptions.Compiled used in normal regex.
        This reduces JIT (Just-In-Time) compilation time, making it more efficient for tokenizing large inputs.

    Type Safety & Maintainability:
        Instead of storing regex as strings in a list, each pattern is defined as a partial static method with[GeneratedRegex].
        The compiler ensures these regexes are correct, avoiding typos in string-based regex definitions.
*/