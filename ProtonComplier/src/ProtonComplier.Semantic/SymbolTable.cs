// <copyright file="SymbolTable.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Semantic
{
    using System.Text;
    using Proton.ErrorHandler;
    using Proton.Lexer;
    using ProtonComplier.Semantic;

    /// <summary>
    /// Represents a symbol table used in semantic analysis.
    /// Stores and manages declared symbols (e.g., variables) in the current scope.
    /// </summary>
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> symbols;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolTable"/> class.
        /// </summary>
        public SymbolTable()
        {
            this.symbols = new ();
        }

        /// <summary>
        /// Gets an enumerable collection of all symbols currently stored in the symbol table.
        /// This provides read-only access to the internal symbol definitions.
        /// </summary>
        public IEnumerable<Symbol> Symbols => this.symbols.Values;

        /// <summary>
        /// Adds a symbol to the symbol table.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        public void AddSymbol(Symbol symbol)
        {
            // Chehck if symbol with the same name already exists in the table
            if (this.symbols.ContainsKey(symbol.Name))
            {
                // Generate error message
                throw new AnalyzerError(
                "207",
                string.Format(MessageRegistry.GetMessage(207).Text, symbol.Name, symbol.SymbolLine, symbol.SymbolColumn));
            }

            this.symbols[symbol.Name] = symbol;
        }

        /// <summary>
        /// Finds and returns a symbol by its name.
        /// </summary>
        /// <param name="symbol">The token of the symbol to find.</param>
        /// <returns>The found <see cref="Symbol"/> instance, or null if not found.</returns>
        public Symbol? FindSymbol(Token symbol)
        {
            if (this.SymbolExists(symbol.TokenValue))
            {
                return this.symbols[symbol.TokenValue];
            }
            else
            {
                // Generate error message
                throw new AnalyzerError(
                    "209",
                    string.Format(MessageRegistry.GetMessage(209).Text, symbol.TokenValue, symbol.TokenLine, symbol.TokenColumn));
            }
        }

        /// <summary>
        /// Remove a symbol by its name.
        /// </summary>
        /// <param name="symbol">The token of the symbol to find.</param>
        public void RemoveSymbol(Token symbol)
        {
            if (this.SymbolExists(symbol.TokenValue))
            {
                this.symbols.Remove(symbol.TokenValue);
            }
            else
            {
                // Generate error message
                throw new AnalyzerError(
                    "209",
                    string.Format(MessageRegistry.GetMessage(209).Text, symbol.TokenValue, symbol.TokenLine, symbol.TokenColumn));
            }
        }

        /// <summary>
        /// Updates an existing symbol in the symbol table.
        /// Throws an error if the symbol does not exist.
        /// </summary>
        /// <param name="symbol">The updated symbol instance.</param>
        public void UpdateSymbol(Symbol symbol)
        {
            if (!this.SymbolExists(symbol.Name))
            {
                // Generate error message
                throw new AnalyzerError(
                    "209",
                    string.Format(MessageRegistry.GetMessage(209).Text, symbol.Name, symbol.SymbolLine, symbol.SymbolColumn));
            }

            this.symbols[symbol.Name] = symbol;
        }

        /// <summary>
        /// Marks a symbol as initialized or used by its name.
        /// Useful for tracking variable initialization during semantic analysis.
        /// </summary>
        /// <param name="symbol">The updated symbol instance.</param>
        /// <returns>True if the symbol already initialized; otherwise, false.</returns>
        public bool MarkSymbolAsUsed(Symbol symbol)
        {
            var markedSymbol = this.symbols[symbol.Name];

            // Chehck if already initialized
            if (markedSymbol.IsInitialized == true)
            {
                return true; // Indicated to add warning
            }
            else
            {
                this.symbols[symbol.Name].IsInitialized = true;
                return false;
            }
        }

        /// <summary>
        /// Clears all symbols from the symbol table.
        /// </summary>
        public void Clear()
        {
            this.symbols.Clear();
        }

        /// <summary>
        /// Returns a formatted table of all symbols currently stored in the symbol table.
        /// Each symbol is displayed with its name, type, category, position, list status, initialization state, value, and result status.
        /// If no symbols are found, a default message is returned.
        /// </summary>
        /// <returns>A string representing the formatted symbol table or a message indicating no symbols were found.</returns>
        public string DisplaySymbols()
        {
            if (this.symbols.Count == 0)
            {
                return "No symbols were found in the semantic table.";
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(
                $"| {{0,-{SymbolTableFormat.NameWidth}}} | {{1,-{SymbolTableFormat.TypeWidth}}} | " +
                $"{{2,-{SymbolTableFormat.CategoryWidth}}} | {{3,-{SymbolTableFormat.LineWidth}}} | " +
                $"{{4,-{SymbolTableFormat.ColWidth}}} | {{5,-{SymbolTableFormat.ListWidth}}} | " +
                $"{{6,-{SymbolTableFormat.InitializedWidth}}} | {{7,-{SymbolTableFormat.ValueWidth}}} | " +
                $"{{8,-{SymbolTableFormat.ValueWidth}}} | {{9,-{SymbolTableFormat.ResultWidth}}} |",
                "Name", "Type", "Category", "Line", "Col", "List", "Initialized", "Value", "Tokens", "Result"));

            int totalWidth = SymbolTableFormat.NameWidth +
                             SymbolTableFormat.TypeWidth +
                             SymbolTableFormat.CategoryWidth +
                             SymbolTableFormat.LineWidth +
                             SymbolTableFormat.ColWidth +
                             SymbolTableFormat.ListWidth +
                             SymbolTableFormat.InitializedWidth +
                             SymbolTableFormat.ValueWidth * 2 +  // for value and valueTokens
                             SymbolTableFormat.ResultWidth +
                             11 * 3 + 1; // padding and dividers

            sb.AppendLine(new string('-', totalWidth));

            foreach (var symbol in this.symbols)
            {
                sb.AppendLine(symbol.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if a symbol with the given name exists in the symbol table.
        /// </summary>
        /// <param name="name">The name of the symbol to check for.</param>
        /// <returns>True if the symbol exists; otherwise, false.</returns>
        private bool SymbolExists(string name)
        {
            return this.symbols.ContainsKey(name);
        }
    }
}
