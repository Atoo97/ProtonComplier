// <copyright file="ParseNode.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.Parser
{
    using Proton.Lexer;

    /// <summary>
    /// Represents a node in the parse tree used during syntax analysis.
    /// Each node may contain a single token or a list of tokens, and optional left and right child nodes.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ParseNode"/> class with a single token.
    /// </remarks>
    /// <param name="symbol">The token this node represents.</param>
    public abstract class ParseNode(Token symbol)
    {
        /// <summary>
        /// Gets or sets the primary token represented by this parse tree node.
        /// </summary>
        public Token ParseSymbol { get; set; } = symbol;

        /// <summary>
        /// Gets the left child node in the parse tree.
        /// </summary>
        public ParseNode? LeftNode { get; private set; }

        /// <summary>
        /// Gets the right child node in the parse tree.
        /// </summary>
        public ParseNode? RightNode { get; private set; }

        /// <summary>
        /// Sets the left child of this node. Can be overridden by derived classes.
        /// </summary>
        /// <param name="child">The node to set as the left child.</param>
        public void SetLeft(ParseNode child)
        {
            this.LeftNode = child;
        }

        /// <summary>
        /// Sets the right child of this node. Can be overridden by derived classes.
        /// </summary>
        /// <param name="child">The node to set as the right child.</param>
        public void SetRight(ParseNode child)
        {
            this.RightNode = child;
        }

        /// <summary>
        /// Converts the contents of the token list to a formatted code string.
        /// </summary>
        /// <param name="ident">Set the ident size of display.</param>
        /// <returns>A string representation of the node's token values.</returns>
        public virtual string ToCode(int ident)
        {
            return $"ParseNode: [TokenType:{this.ParseSymbol!.TokenType} | TokenValue:{this.ParseSymbol!.TokenValue}]";
        }
    }
}
