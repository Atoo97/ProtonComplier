// <copyright file="SummationExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using Proton.Lexer.Enums;
    using ProtonComplier.Parser.Statements;

    public class SummationExpression : Expression
    {
        public SummationExpression(VariableInitializationExpression var, Expression whilenum, Expression opt)
            : base(whilenum.ParseSymbol)
        {
            this.LeftExpression = var;
            this.MiddleExpression = whilenum;
            this.RightExpression = opt;
        }

        /// <summary>
        /// Gets the right-hand expression of the Summation operation.
        /// </summary>
        public VariableInitializationExpression LeftExpression { get; private set; }

        /// <summary>
        /// Gets the right-hand expression of the Summation operation.
        /// </summary>
        public Expression MiddleExpression { get; private set; }

        /// <summary>
        /// Gets the right-hand expression of the Summation operation.
        /// </summary>
        public Expression RightExpression { get; private set; }

        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"SummationExpression:\n" +
                  $"{indentLine}├─ VariableInitialization: {this.LeftExpression.ToCode(ident + 1)}\n" +
                  $"{indentLine}├─ While: {this.MiddleExpression.ToCode(ident + 1)}" +
                  $"{indentLine}└─ Option: {this.RightExpression.ToCode(ident + 1)}";
        }
    }
}
