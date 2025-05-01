// <copyright file="TypeMapping.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.CodeGenerator
{
    /// <summary>
    /// Represents a type mapping from custom language types to C# code types.
    /// </summary>
    public static class TypeMapping
    {
        private static readonly Dictionary<string, string> TypeMap = new ()
        {
            { "Natural", "uint" },      // Natural number
            { "Integer", "int" },       // Integer number
            { "Real", "double" },       // Real number
        };

        /// <summary>
        /// Converts a custom type name (e.g., "N", "R") to its equivalent C# type.
        /// </summary>
        /// <param name="customType">The type from the custom language.</param>
        /// <returns>The equivalent C# type as a string.</returns>
        public static string ToCSharpType(string customType)
        {
            if (TypeMap.TryGetValue(customType, out var csharpType))
            {
                return csharpType;
            }

            // If the type is unknown
            return null!;
        }
    }
}
