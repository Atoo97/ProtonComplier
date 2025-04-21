// <copyright file="Message.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.ErrorHandler
{
    /// <summary>
    /// Represents a structured message used in analysis exceptions.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// </remarks>
    /// <param name="id">ID of the message.</param>
    /// <param name="text">text of the message.</param>
    public class Message(string id, string text)
    {
        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// Gets the text of the message.
        /// </summary>
        public string Text { get; } = text;
    }
}
