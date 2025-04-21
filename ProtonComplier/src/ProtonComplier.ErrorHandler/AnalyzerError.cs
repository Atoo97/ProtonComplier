// <copyright file="AnalyzerError.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.ErrorHandler
{
    /// <summary>
    /// Represents a error that occurs during analysis in the Proton project.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AnalyzerError"/> class with the specified ID and message.
    /// </remarks>
    /// <param name="id">The unique identifier for the error.</param>
    /// <param name="message">The descriptive message for the error.</param>
    public class AnalyzerError(string id, string message)
        : BaseException(id, message)
    {
        /// <summary>
        /// Gets the type of error (in this case, always "WARNING").
        /// </summary>
        public override string? ErrorType { get; } = "ERROR";

        /// <summary>
        /// Represents the current AnalyzerError.
        /// </summary>
        /// <returns>Returns a string that represents the current AnalyzerError.</returns>
        public override string ToString()
        {
            return $"[{this.ErrorType}] ({this.ID}) {this.Message}";
        }
    }
}
