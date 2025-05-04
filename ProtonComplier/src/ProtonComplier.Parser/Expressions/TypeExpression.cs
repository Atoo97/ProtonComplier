// <copyright file="TypeExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a type expression in the syntax tree, such as "N", "R",
    /// with support for detecting if it represents a list (e.g., "N[]").
    /// </summary>
    public class TypeExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeExpression"/> class.
        /// Validates that the provided token is a valid type token.
        /// </summary>
        /// <param name="type">The type token (e.g., "N", "R").</param>
        /// <param name="isList">Indicates whether the type is a list type (e.g., "N[]").</param>
        /// <exception cref="Exception">Thrown if the token is not a recognized type token.</exception>
        public TypeExpression(Token type, bool isList)
            : base(type)
        {
            if (type.TokenCategory != TokenCategory.Keyword)
            {
                // Generate error message
                throw new AnalyzerError(
                    "104",
                    string.Format(MessageRegistry.GetMessage(104).Text, type.TokenValue, type.TokenLine, type.TokenColumn, TokenCategory.Keyword));
            }

            this.IsList = isList;
        }

        /// <summary>
        /// Gets a value indicating whether the type represents a list (e.g., "N[]").
        /// </summary>
        public bool IsList { get; private set; }

        /// <summary>
        /// Generates the string representation of the type expression for code generation,
        /// including a "[]" suffix if the type represents a list. Indentation is applied based on the specified level.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the type expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            if (this.IsList)
            {
                return $"{indentLine}Type[]:{base.ToCode(ident)}";
            }

            return $"{indentLine}Type: {base.ToCode(ident)}";
        }
    }
}
