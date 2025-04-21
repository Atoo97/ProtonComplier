// <copyright file="MacroType.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Lexer.Macro
{
    /// <summary>
    /// Represents predefined macro section types.
    /// </summary>
    public enum MacroTypes
    {
        /// <summary>
        /// Represents predefined macro section types used in the context of macros.
        /// </summary>
        StateSpace,

        /// <summary>
        /// Represents the input section of a macro.
        /// </summary>
        Input,

        /// <summary>
        /// Represents the precondition section of a macro.
        /// </summary>
        Precondition,

        /// <summary>
        /// Represents the postcondition section of a macro.
        /// </summary>
        Postcondition,
    }

    /// <summary>
    /// Provides mappings between <see cref="MacroType"/> values and their corresponding string representations.
    /// </summary>
    public class MacroType
    {
        /// <summary>
        /// A mapping of MacroType to their respective string values.
        /// </summary>
        public static readonly IReadOnlyDictionary<MacroTypes, string> ExpectedMacros = new Dictionary<MacroTypes, string>
        {
            { MacroTypes.StateSpace,     "StateSpace" },
            { MacroTypes.Input,          "Input" },
            { MacroTypes.Precondition,   "Precondition" },
            { MacroTypes.Postcondition,  "Postcondition" },
        };
    }
}
