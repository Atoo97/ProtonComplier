// <copyright file="PreconditionDeclaration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser.Statements
{
    using Proton.ErrorHandler;
    using Proton.Parser.Expressions;

    public class PreconditionDeclaration : Statement
    {
        public PreconditionDeclaration(Expression exp)
            : base(exp)
        {
            SetRight(exp);
        }
        
        public override string ToCode()
        {
            return $"\nPreconditionDeclaration\n" +
                   $" └─ Conditiion: {RightNode!.ToCode(1)}\n";
        }
    }
}
