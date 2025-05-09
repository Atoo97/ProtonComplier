// <copyright file="Symbol.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
    using Proton.Lexer;
    using Proton.Lexer.Enums;
    using ProtonComplier.Semantic;
    using System.Text;

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
        /// Gets or sets the list of tokens representing the value(s) assigned to the symbol as string.
        /// For array or list-like assignments, multiple tokens may be present.
        /// </summary>
        public StringBuilder ValueTokens { get; set; } = new ();

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
        /// Returns a detailed string representation of the symbol for symbol table display.
        /// </summary>
        /// <returns>A formatted string showing the symbol's properties as a table row.</returns>
        public override string ToString()
        {
            string valueStr = this.Value != null && this.Value.Count > 0
                ? string.Join(", ", this.Value.Select(v => v.TokenValue))
                : "N/A";

            string valueTokensStr = this.ValueTokens.Length > 0
                ? this.ValueTokens.ToString()
                : "N/A";

            return string.Format(
                $"| {{0,-{SymbolTableFormat.NameWidth}}} | {{1,-{SymbolTableFormat.TypeWidth}}} | " +
                $"{{2,-{SymbolTableFormat.CategoryWidth}}} | {{3,-{SymbolTableFormat.LineWidth}}} | " +
                $"{{4,-{SymbolTableFormat.ColWidth}}} | {{5,-{SymbolTableFormat.ListWidth}}} | " +
                $"{{6,-{SymbolTableFormat.InitializedWidth}}} | {{7,-{SymbolTableFormat.ValueWidth}}} | " +
                $"{{8,-{SymbolTableFormat.ValueWidth}}} |",
                this.Name,
                this.Type,
                this.Category,
                this.SymbolLine,
                this.SymbolColumn,
                this.IsList ? "Yes" : "No",
                this.IsInitialized ? "Yes" : "No",
                valueStr,
                valueTokensStr);
        }
    }
}
