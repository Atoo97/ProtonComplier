// <copyright file="Symbol.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a symbol (such as a variable) in the semantic model.
    /// Stores its name, type, value(s), and initialization status.
    /// </summary>
    public class Symbol
    {
        /// <summary>
        /// Gets or sets the token name representing the name of the symbol (e.g., variable identifier).
        /// </summary>
        required public string Name { get; set; }

        /// <summary>
        /// Gets or sets the token type representing the data type of the symbol (e.g., Int, Uint, Double).
        /// </summary>
        required public TokenType Type { get; set; }

        /// <summary>
        /// Gets or sets the token type representing the data category of the symbol (e.g., Int, Uint, Double).
        /// </summary>
        required public TokenCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the list of tokens representing the value(s) assigned to the symbol.
        /// For array or list-like assignments, multiple tokens may be present.
        /// </summary>
        required public List<Token> Value { get; set; }

        /// <summary>
        /// Gets or Sets the line number where the symbol was found in the source code.
        /// </summary>
        required public int SymbolLine { get; set; }

        /// <summary>
        /// Gets or Sets the column number where the symbol was found in the source code.
        /// </summary>
        required public int SymbolColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the symbol is list.
        /// </summary>
        required public bool IsList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the symbol has been initialized.
        /// </summary>
        public bool IsInitialized { get; set; } = false;

        /// <summary>
        /// Marks the symbol as initialized.
        /// Useful for tracking usage during semantic analysis.
        /// </summary>
        public void MarkAsInitialized()
        {
            this.IsInitialized = true;
        }

        /// <summary>
        /// Returns a string representation of the symbol, showing its name and type.
        /// Useful for debugging and diagnostics.
        /// </summary>
        /// <returns>A string representing the symbol's name and type.</returns>
        public override string ToString()
        {
            return $"{this.Name} : {this.Type}";
        }

        // Additional properties can be added as necessary, such as:
        // - Value (for constants or literals)
        // - Line Number (where it is declared)
        // - IsParameter (if it's a function parameter)
    }
}
