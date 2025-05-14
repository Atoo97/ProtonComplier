// <copyright file="VariableInitializationExpression.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Expressions
{
    /// <summary>
    /// Represents a variable initialization expression.
    /// This includes a variable identifier, assign (like "="), and type definier (like "{1,2,(2+5),4..}").
    /// (e.g., "_var1,_var2=2+(4-6);).
    /// </summary>
    public class VariableInitializationExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableInitializationExpression"/> class.
        /// This constructor processes the identifier, separator, and type specifier for the variable declaration.
        /// </summary>
        /// <param name="identifier">The identifier expression for the variable.</param>
        /// <param name="separator">The separator expression (e.g., ":=).</param>
        /// <param name="valueSpecifier">The type specifier expression (e.g., "{1,2,3,4};").</param>
        /// <exception cref="Exception">Thrown if there is an error in creating the variable declaration components.</exception>
        public VariableInitializationExpression(IdentifierExpression identifier, AssignExpression separator, Expression valueSpecifier)
            : base(separator.ParseSymbol)
        {
            // Set the left and right children for the this statement
            this.SetLeft(identifier);
            this.SetRight(valueSpecifier);
        }

        /// <summary>
        /// Generates the string representation of the initialization expression for code generation,
        /// including the assignment operator and proper valueSpecifier.
        /// </summary>
        /// <param name="ident">The indentation level (each level equals 4 spaces).</param>
        /// <returns>A formatted string representing the assignment expression.</returns>
        public override string ToCode(int ident)
        {
            string indentLine = new (' ', ident * 4);
            return $"\nVariableInitializationExpression\n" +
                  $" ├─ Identifier:\n" +
                  $" │  └─ {this.LeftNode!.ToCode(0)}\n" +
                  $" ├─ Assign: {this.ToCode(0)}\n" +
                  $" └─ Valuedefinition:\n" +
                  $"    └─ {this.RightNode!.ToCode(1)}";
        }
    }
}
