// <copyright file="RegexGrammar.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer
{
    using System.Text.RegularExpressions;
    using Proton.Lexer.Enums;

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
                new (TokenType.Comment, TokenCategory.Special, CommentRegex()),

                // Identifier Tokens
                new (TokenType.Options, TokenCategory.Identifier, OptionsRegex()),
                new (TokenType.Min, TokenCategory.Identifier, MinRegex()),
                new (TokenType.Max, TokenCategory.Identifier, MaxRegex()),
                new (TokenType.Length, TokenCategory.Identifier, LengthRegex()),

                // Keyword Tokens
                new (TokenType.Natural, TokenCategory.Keyword, NatIdentRegex()),
                new (TokenType.Integer, TokenCategory.Keyword, IntIdentRegex()),
                new (TokenType.Real, TokenCategory.Keyword, RealIdentRegex()),
                new (TokenType.Character, TokenCategory.Keyword, CharIdentRegex()),
                new (TokenType.Text, TokenCategory.Keyword, StrIdentRegex()),
                new (TokenType.Boolean, TokenCategory.Keyword, BoolIdentRegex()),

                // Literal Tokens
                new (TokenType.Double, TokenCategory.Literal, DoubleRegex()),
                new (TokenType.Uint, TokenCategory.Literal, UIntegerRegex()),
                new (TokenType.Int, TokenCategory.Literal, IntegerRegex()),

                new (TokenType.Identifier, TokenCategory.Identifier, IdentifierRegex()), // Identifier token

                new (TokenType.Char, TokenCategory.Literal, CharacterRegex()),
                new (TokenType.String, TokenCategory.Literal, StringRegex()),
                new (TokenType.Bool, TokenCategory.Literal, BooleanRegex()),

                // Operator Tokens
                new (TokenType.Addition, TokenCategory.Operator, AdditionRegex()),
                new (TokenType.Subtraction, TokenCategory.Operator, SubtractionRegex()),
                new (TokenType.Multiplication, TokenCategory.Operator, MultiplicationRegex()),
                new (TokenType.Division, TokenCategory.Operator, DivisionRegex()),
                new (TokenType.Modulus, TokenCategory.Operator, ModulusRegex()),
                new (TokenType.Equal, TokenCategory.Operator, EqualRegex()),
                new (TokenType.Assign, TokenCategory.Operator, AssignRegex()),
                new (TokenType.NotEqual, TokenCategory.Operator, NotEqualRegex()),
                new (TokenType.GreaterThan, TokenCategory.Operator, GreaterThanRegex()),
                new (TokenType.LessThan, TokenCategory.Operator, LessThanRegex()),
                new (TokenType.GreaterThanOrEqual, TokenCategory.Operator, GreaterThanOrEqualRegex()),
                new (TokenType.LessThanOrEqual, TokenCategory.Operator, LessThanOrEqualRegex()),
                new (TokenType.LogicalAnd, TokenCategory.Operator, LogicalAndRegex()),
                new (TokenType.LogicalOr, TokenCategory.Operator, LogicalOrRegex()),
                new (TokenType.LogicalNot, TokenCategory.Operator, LogicalNotRegex()),
                new (TokenType.Implication, TokenCategory.Operator, ImplicationRegex()),
                new (TokenType.UniversalQuant, TokenCategory.Operator, UniversalQuantRegex()),
                new (TokenType.ExistentialQuant, TokenCategory.Operator, ExistentialQuantRegex()),
                new (TokenType.Product, TokenCategory.Operator, ProductRegex()),
                new (TokenType.Summation, TokenCategory.Operator, SummationRegex()),

                // Punctuator Tokens
                new (TokenType.Semicolon, TokenCategory.Punctuator, SemicolonRegex()),
                new (TokenType.Colon, TokenCategory.Punctuator, ColonRegex()),
                new (TokenType.Comma, TokenCategory.Punctuator, CommaRegex()),
                new (TokenType.OpenParen, TokenCategory.Punctuator, OpenParenRegex()),
                new (TokenType.CloseParen, TokenCategory.Punctuator, CloseParenRegex()),
                new (TokenType.OpenBrace, TokenCategory.Punctuator, OpenBraceRegex()),
                new (TokenType.CloseBrace, TokenCategory.Punctuator, CloseBraceRegex()),
                new (TokenType.ListSpecifier, TokenCategory.Punctuator, ListSpecifierRegex()),
                new (TokenType.OpenSqrBrace, TokenCategory.Punctuator, OpenSqrBraceRegex()),
                new (TokenType.CloseSqrBrace, TokenCategory.Punctuator, CloseSqrBraceRegex()),

                // Special Tokens
                new (TokenType.Macro, TokenCategory.Special, MacroRegex()),
                new (TokenType.QuestionMarks, TokenCategory.Special, QuestionMarksRegex()),
                new (TokenType.Period, TokenCategory.Special, PeriodRegex()),
                new (TokenType.Whitespace, TokenCategory.Special, WhitespaceRegex()),

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

        [GeneratedRegex("\\G(?<![\\w\\d])(-?(?!-)(0|[1-9][0-9]*))", RegexOptions.Compiled)]
        private static partial Regex IntegerRegex(); // Matches integers, ensuring the minus sign is valid

        [GeneratedRegex("\\G(?<![\\w\\d])(-?(?!-)(0|[1-9][0-9]*)\\.[0-9]+)", RegexOptions.Compiled)]
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