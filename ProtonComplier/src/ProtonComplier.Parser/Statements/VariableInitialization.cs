// <copyright file="VariableInitialization.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace ProtonComplier.Parser.Statements
{
    using Proton.ErrorHandler;
    using Proton.Parser.Expressions;
    using Proton.Parser.Statements;
    using ProtonComplier.Parser.Expressions;

    /// <summary>
    /// Represents a variable initialization statement in the parse tree.
    /// This includes a variable identifier, assign (like "="), and type definier (like "{1,2,(2+5),4..}").
    /// (e.g., "_var1,_var2=2+(4-6);).
    /// </summary>
    public class VariableInitialization : Statement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableInitialization"/> class.
        /// This constructor processes the identifier, separator, and type specifier for the variable declaration.
        /// </summary>
        /// <param name="identifier">The identifier expression for the variable.</param>
        /// <param name="separator">The separator expression (e.g., ":=).</param>
        /// <param name="valueSpecifier">The type specifier expression (e.g., "{1,2,3,4};").</param>
        /// <exception cref="Exception">Thrown if there is an error in creating the variable declaration components.</exception>
        public VariableInitialization(IdentifierExpression identifier, AssignExpression separator, Expression valueSpecifier)
            : base(separator)
        {
            // Set the left and right children for the this statement
            this.SetLeft(identifier);
            this.SetRight(valueSpecifier);

            if (valueSpecifier is ListExpression)
            {
                this.IsList = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the initialized identifier represents a list (e.g., "{....}").
        /// </summary>
        public bool IsList { get; private set; } = false;

        /// <summary>
        /// Converts the variable initialization statement to its string representation for code generation.
        /// This method formats the statement into a structured string, including the identifier, assignment, and type.
        /// </summary>
        /// <returns>A string representation of the variable initialization statement.</returns>
        public override string ToCode()
        {
            return $"\nVariableInitialization\n" +
                   $" ├─ Identifier:\n" +
                   $" │  └─ {this.LeftNode!.ToCode(0)}\n" +
                   $" ├─ Assign: {this.ToCode(0)}\n" +
                   $" └─ Valuedefinition:\n" +
                   $"    └─ {this.RightNode!.ToCode(1)}";
        }
    }
}
