// <copyright file="BaseException.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.ErrorHandler
{
    /// <summary>
    /// Represents the base class for errors and warnings encountered during analysis in the Proton project.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BaseException"/> class with the specified ID and message.
    /// </remarks>
    /// <param name="id">The unique identifier for the error or warning.</param>
    /// <param name="message">The descriptive message for the error or warning.</param>
    public class BaseException(string id, string message)
        : Exception
    {
        /// <summary>
        /// Gets the unique identifier for the error or warning.
        /// </summary>
        public string ID { get; } = id;

        /// <summary>
        /// Gets the descriptive message for the error or warning.
        /// </summary>
        public override string Message { get; } = message;

        /// <summary>
        /// Gets the type of error or warning (e.g., "WARNING" or "ERROR").
        /// </summary>
        public virtual string? ErrorType { get; }
    }
}
