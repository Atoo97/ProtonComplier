// <copyright file="SymbolTableFormat.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace ProtonComplier.Semantic
{
    /// <summary>
    /// Defines constant column widths for formatting the symbol table display in a consistent and aligned manner.
    /// Each constant corresponds to the character width of a specific column, such as Name, Type, Line, etc.
    /// Used primarily when generating textual representations of the semantic symbol table.
    /// </summary>
    public static class SymbolTableFormat
    {
        /// <summary>
        /// The column width allocated for the symbol name.
        /// </summary>
        public const int NameWidth = 20;

        /// <summary>
        /// The column width allocated for the symbol's data type (e.g., Int, Double).
        /// </summary>
        public const int TypeWidth = 10;

        /// <summary>
        /// The column width allocated for the symbol's category (e.g., Numeric, Identifier).
        /// </summary>
        public const int CategoryWidth = 12;

        /// <summary>
        /// The column width allocated for the symbol's source code line number.
        /// </summary>
        public const int LineWidth = 6;

        /// <summary>
        /// The column width allocated for the symbol's source code column number.
        /// </summary>
        public const int ColWidth = 6;

        /// <summary>
        /// The column width allocated to display whether the symbol represents a list.
        /// </summary>
        public const int ListWidth = 5;

        /// <summary>
        /// The column width allocated for the symbol's initialization status (e.g., Yes/No).
        /// </summary>
        public const int InitializedWidth = 12;

        /// <summary>
        /// The column width allocated for displaying the symbol's value(s).
        /// </summary>
        public const int ValueWidth = 20;
    }
}
