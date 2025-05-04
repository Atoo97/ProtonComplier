// <copyright file="Expression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.Lexer;

    /// <summary>
    /// Represents the base class for all expression nodes in the parse tree.
    /// </summary>
    public abstract class Expression : ParseNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class using a single token.
        /// </summary>
        /// <param name="symbol">The token that represents the expression.</param>
        protected Expression(Token symbol)
            : base(symbol)
        {
        }

        /// <summary>
        /// Converts the expression to its string representation for code generation.
        /// </summary>
        /// <param name="ident">Set the ident size of display.</param>
        /// <returns>A string representation of the expression prefixed with "Exp:".</returns>
        public override string ToCode(int ident)
        {
            return $"Expression: {base.ToCode(ident)}";
        }
    }
}

/*
 A program is a sequence of statements.
 Each statement is an instruction for the computer to do something.
 Statements are the rigid structure that holds our program together, while expressions fill in the details.
 // Statement:  let hi =  some expression;

        1                           → produces 1
        "hello"                     → produces "hello"
        5 * 10                      → produces 50
        num > 100                   → produces either true or false
        isHappy ? "🙂" : "🙁"      → produces an emoji

        (5 + 1) * 2 how many expr? => 2                     ?? => 5 db
                                      1
                                      5
                                      (5 + 1)
                                      (5 + 1) * 2
*/