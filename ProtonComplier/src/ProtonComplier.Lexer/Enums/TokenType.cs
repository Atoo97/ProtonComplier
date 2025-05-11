// <copyright file="TokenType.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer.Enums
{
    /// <summary>
    /// Extended token types for Proton language.
    /// </summary>
    public enum TokenType
    {
        // ******************************************//
        //              Keyword Tokens               //
        // ******************************************//

        /// <summary>
        /// N (uint) = 0, 1, 2, 3, 4, 5..
        /// </summary>
        Natural,

        /// <summary>
        /// Z (int)  = .. −2, −1, 0, 1, 2..
        /// </summary>
        Integer,

        /// <summary>
        /// R (double) = .. −2.1, −1.5, 0, 1.6, 2.9..
        /// </summary>
        Real,

        /// <summary>
        /// C (char)
        /// </summary>
        Character,

        /// <summary>
        /// $ (string)
        /// </summary>
        Text,

        /// <summary>
        /// B (bool)
        /// </summary>
        Boolean,

        // ******************************************//
        //              Identifier Tokens            //
        // ******************************************//

        /// <summary>
        /// Variable names (e.g. _varName, _number, Num)
        /// </summary>
        Identifier,

        /// <summary>
        /// (Opt) - option for conditional statements
        /// </summary>
        Options,

        /// <summary>
        ///  Min(a,b)
        /// </summary>
        Min,

        /// <summary>
        /// Max(a,b)
        /// </summary>
        Max,

        /// <summary>
        /// Length
        /// </summary>
        Length,

        // ******************************************//
        //              Literal Tokens               //
        // ******************************************//

        /// <summary>
        /// UInt literal (e.g. 0, 1, 2...)
        /// </summary>
        Uint,

        /// <summary>
        /// Integer literal (e.g. 1-1, 0, 42)
        /// </summary>
        Int,

        /// <summary>
        /// Double literal (e.g. 3.14)
        /// </summary>
        Double,

        /// <summary>
        /// Char literal (e.g. 'a')
        /// </summary>
        Char,

        /// <summary>
        /// String literal (e.g. "abc")
        /// </summary>
        String,

        /// <summary>
        /// Boolean literal (e.g. True/False)
        /// </summary>
        Bool,

        // ******************************************//
        //              Operator Tokens              //
        // ******************************************//

        /// <summary>
        /// +
        /// </summary>
        Addition,

        /// <summary>
        /// -
        /// </summary>
        Subtraction,

        /// <summary>
        /// *
        /// </summary>
        Multiplication,

        /// <summary>
        /// /
        /// </summary>
        Division,

        /// <summary>
        /// %
        /// </summary>
        Modulus,

        /// <summary>
        /// =
        /// </summary>
        Assign,

        /// <summary>
        /// ==
        /// </summary>
        Equal,

        /// <summary>
        /// ≠
        /// </summary>
        NotEqual,

        /// <summary>
        /// >
        /// </summary>
        GreaterThan,

        /// <summary>
        /// opposite of >
        /// </summary>
        LessThan,

        /// <summary>
        /// ≥
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// ≤
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// ∧
        /// </summary>
        LogicalAnd,

        /// <summary>
        /// ∨
        /// </summary>
        LogicalOr,

        /// <summary>
        /// ┐
        /// </summary>
        LogicalNot,

        /// <summary>
        /// →
        /// </summary>
        Implication,

        /// <summary>
        /// ∀
        /// </summary>
        UniversalQuant,

        /// <summary>
        /// ∃
        /// </summary>
        ExistentialQuant,

        /// <summary>
        /// ∏
        /// </summary>
        Product,

        /// <summary>
        /// ∑
        /// </summary>
        Summation,

        // ******************************************//
        //              Punctuator Tokens            //
        // ******************************************//

        /// <summary>
        /// ; to terminate statements (optional)
        /// </summary>
        Semicolon,

        /// <summary>
        /// : to assign statements
        /// </summary>
        Colon,

        /// <summary>
        /// , to terminate statements, variables (optional)
        /// </summary>
        Comma,

        /// <summary>
        /// (
        /// </summary>
        OpenParen,

        /// <summary>
        /// )
        /// </summary>
        CloseParen,

        /// <summary>
        /// {}
        /// </summary>
        ValueSpecifier,

        /// <summary>
        /// {
        /// </summary>
        OpenBrace,

        /// <summary>
        /// }
        /// </summary>
        CloseBrace,

        /// <summary>
        /// []
        /// </summary>
        ListSpecifier,

        /// <summary>
        /// [n] or [-n]
        /// </summary>
        ListNthElement,

        /// <summary>
        /// [
        /// </summary>
        OpenSqrBrace,

        /// <summary>
        /// ]
        /// </summary>
        CloseSqrBrace,

        // ******************************************//
        //              Special Tokens               //
        // ******************************************//

        /// <summary>
        /// #   to separate logical parts (#StatePlace..)
        /// </summary>
        Macro,

        /// <summary>
        /// ??  to parameter spaceholder
        /// </summary>
        QuestionMarks,

        /// <summary>
        /// .   to function call on variables
        /// </summary>
        Period,

        /// <summary>
        /// //  one line comments
        /// </summary>
        Comment,

        /// <summary>
        /// any whitespace
        /// </summary>
        Whitespace,

        /// <summary>
        /// NewLine
        /// </summary>
        Newline,

        /// <summary>
        ///  End of input.
        /// </summary>
        EndOfInput,

        /// <summary>
        /// Unknown specaial token
        /// </summary>
        Unknown,
    }
}
