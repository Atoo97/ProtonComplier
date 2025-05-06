// <copyright file="MessageRegistry.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.ErrorHandler
{
    /// <summary>
    /// Maintains a registry of predefined error and warning messages used by analyzers during the processing of the Proton project.
    /// </summary>
    public class MessageRegistry
    {
        /// <summary>
        /// A static dictionary that stores messages indexed by their unique integer ID.
        /// </summary>
        private static readonly Dictionary<int, Message> Messages = new ()
        {
            // Lexer errors & warnings (001-100):
            {
                001, new Message(
                "internal-complie-error",
                "LexerError: during complie Proton project")
            },
            {
                004, new Message(
                "invalid-macro-type",
                "LexerError: Invalid macro: '{0}' at line {1}, column {2}. Expected one of: {3}.")
            },
            {
                005, new Message(
                "missing-macro-section",
                "LexerError: Missing macro section: '{0}'. This section is required for the document to be valid.")
            },
            {
                006, new Message(
                "multiple-macro-definition",
                "LexerWarning: Multiple '{0}' macros detected at line {1}, column {2}. The definitions are being added as extra lines.")
            },
            {
                008, new Message(
                "invalid-value-placement",
                "LexerError: '{0}' at line {1}, column {2} is not valid because the first row does not belong under any macro type.")
            },
            {
                011, new Message(
                "unknown-token-definition",
                "LexerError: The unknown token '{0}' detected at line {1}, column {2} is not exist in the current context.")
            },

            // Parser errors & warnings (101-200):
            {
                101, new Message(
                "invalid-expression-definition",
                "ParserError: Invalid expression at line {0}, column {1}. Expected expression type: '{2}'.")
            },
            {
                104, new Message(
                "unexpected-token",
                "ParserError: Unexpected token: '{0}' at line {1}, column {2}. Expected token: '{3}'.")
            },
            {
                107, new Message(
                "multiple-commas",
                "ParserWarning: Multiple consecutive commas detected at line {0}, column {1}.")
            },
            {
                108, new Message(
                "multiple-delimeter",
                "ParserWarning: Multiple consecutive delimeter detected at line {0}, column {1}.")
            },
            {
                112, new Message(
                "multiple-listspecifier",
                "ParserError: Multiple list specifier detected at line {0}, column {1}.")
            },

            // Semantic errors & warnings (201-300):
            {
                204, new Message(
                "invalid-variable-identifier",
                "SemanticalError: Invalid variable name declaration '{0}' at line {1}, column {2}.")
            },
        };

        /// <summary>
        /// Retrieves a message by its unique integer ID.
        /// </summary>
        /// <param name="id">The unique ID of the message to retrieve.</param>
        /// <returns>A <see cref="Message"/> instance containing the message details.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided ID does not exist in the registry.</exception>
        public static Message GetMessage(int id)
        {
            return Messages.TryGetValue(id, out Message? value) ? value : throw new ArgumentException($"Message with ID '{id}' not found.");
        }
    }
}
