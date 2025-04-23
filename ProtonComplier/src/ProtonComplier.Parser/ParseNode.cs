// <copyright file="ParseNode.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser
{
    using Proton.Lexer;
    using Proton.Lexer.Enums;

    /// <summary>
    /// Represents a node in the parse tree used during syntax analysis.
    /// Each node may contain a single token or a list of tokens, and optional left and right child nodes.
    /// </summary>
    public abstract class ParseNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseNode"/> class with a single token.
        /// </summary>
        /// <param name="symbol">The token this node represents.</param>
        public ParseNode(Token symbol)
        {
            this.ParseSymbol = symbol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseNode"/> class with a list of tokens.
        /// </summary>
        /// <param name="symbols">The list of tokens this node represents.</param>
        public ParseNode(List<Token> symbols)
        {
            this.ParseSymbolsList = new List<Token>(symbols);
        }

        /// <summary>
        /// Gets or sets the primary token represented by this parse tree node.
        /// </summary>
        public Token? ParseSymbol { get; set; }

        /// <summary>
        /// Gets or sets the list of tokens associated with this node, used when the node represents a collection of tokens.
        /// </summary>
        public List<Token>? ParseSymbolsList { get; set; }

        /// <summary>
        /// Gets the left child node in the parse tree.
        /// </summary>
        public ParseNode? Left { get; private set; }

        /// <summary>
        /// Gets the right child node in the parse tree.
        /// </summary>
        public ParseNode? Right { get; private set; }

        /// <summary>
        /// Sets the left child of this node. Can be overridden by derived classes.
        /// </summary>
        /// <param name="child">The node to set as the left child.</param>
        public virtual void SetLeft(ParseNode child)
        {
            this.Left = child;
        }

        /// <summary>
        /// Sets the right child of this node. Can be overridden by derived classes.
        /// </summary>
        /// <param name="child">The node to set as the right child.</param>
        public virtual void SetRight(ParseNode child)
        {
            this.Right = child;
        }

        /// <summary>
        /// Converts the contents of the token list to a formatted code string.
        /// </summary>
        /// <returns>A string representation of the node's token values.</returns>
        public virtual string ToCode()
        {
            var symbolsCode = string.Join(", ", this.ParseSymbolsList!
                .Where(symbol => symbol.TokenType != TokenType.Comma)
                .Select(symbol => symbol.TokenValue));

            return symbolsCode;
        }
    }
}
