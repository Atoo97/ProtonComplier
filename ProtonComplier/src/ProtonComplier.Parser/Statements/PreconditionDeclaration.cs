// <copyright file="PreconditionDeclaration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Statements
{
    using Proton.Parser.Expressions;

    /// <summary>
    /// Represents a precondition statement in the parse tree.
    /// Typically used to define a condition that must be true before executing a block or action.
    /// </summary>
    public class PreconditionDeclaration : Statement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreconditionDeclaration"/> class.
        /// </summary>
        /// <param name="exp">The boolean expression representing the precondition condition.</param>
        public PreconditionDeclaration(Expression exp)
            : base(exp)
        {
            this.SetRight(exp);
        }

        /// <summary>
        /// Returns a formatted string representation of the precondition for debugging or visualization.
        /// </summary>
        /// <returns>A string showing the condition expression of the precondition.</returns>
        public override string ToCode()
        {
            return $"\nPreconditionDeclaration\n" +
                   $" └─ Conditiion: {this.RightNode!.ToCode(1)}\n";
        }
    }
}
