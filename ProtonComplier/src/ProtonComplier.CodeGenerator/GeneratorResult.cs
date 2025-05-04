// <copyright file="GeneratorResult.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator
{
    /// <summary>
    /// Represents the result of code generator.
    /// </summary>
    public readonly record struct GeneratorResult(
        string code,
        string result,
        List<string> errors,
        bool isSuccessful);
}
