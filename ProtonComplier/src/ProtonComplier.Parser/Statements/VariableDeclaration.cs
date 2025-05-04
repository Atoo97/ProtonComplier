// <copyright file="VariableDeclaration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Statements
{
    using Proton.ErrorHandler;
    using Proton.Parser.Expressions;

    /// <summary>
    /// Represents a variable declaration statement in the parse tree.
    /// This includes a variable identifier, separator (like ":"), and type specifier (like "N[];").
    /// (e.g., "_var1,_var2:N[]; or _var1:N or _var1:Z[];;").
    /// </summary>
    public class VariableDeclaration : Statement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDeclaration"/> class.
        /// This constructor processes the identifier, separator, and type specifier for the variable declaration.
        /// </summary>
        /// <param name="identifier">The identifier expression for the variable.</param>
        /// <param name="separator">The separator expression (e.g., ":").</param>
        /// <param name="typeSpecifier">The type specifier expression (e.g., "N[];").</param>
        /// <exception cref="Exception">Thrown if there is an error in creating the variable declaration components.</exception>
        public VariableDeclaration(IdentifierExpression identifier, SeparatorExpression separator, TypeSpecifierExpression typeSpecifier)
            : base(separator)
        {
            // Set the left and right children for the this statement
            this.SetLeft(identifier);
            this.SetRight(typeSpecifier);

            this.IsList = typeSpecifier.IsList;
        }

        /// <summary>
        /// Gets a value indicating whether the statement represents a list (e.g., "N[]").
        /// </summary>
        public bool IsList { get; private set; }

        /// <summary>
        /// Converts the variable declaration statement to its string representation for code generation.
        /// This method formats the statement into a structured string, including the identifier, separator, and type.
        /// </summary>
        /// <returns>A string representation of the variable declaration statement.</returns>
        public override string ToCode()
        {
            return $"\nVariableDeclaration\n" +
                   $" ├─ Identifier:\n" +
                   $" │  └─ {this.LeftNode!.ToCode(0)}\n" +
                   $" ├─ Separator: {this.ToCode(0)}\n" +
                   $" └─ Typedefinition:\n" +
                   $"    └─ {this.RightNode!.ToCode(0)}";
        }
    }
}
