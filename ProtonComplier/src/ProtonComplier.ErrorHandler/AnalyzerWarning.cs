// <copyright file="AnalyzerWarning.cs" company="ProtonComplier">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Proton.ErrorHandler
{
    /// <summary>
    /// Represents a warning that occurs during analysis in the Proton project.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AnalyzerWarning"/> class with the specified ID and message.
    /// </remarks>
    /// <param name="id">The unique identifier for the warning.</param>
    /// <param name="message">The descriptive message for the warning.</param>
    public class AnalyzerWarning(string id, string message)
        : BaseException(id, message)
    {
        /// <summary>
        /// Gets the type of warning (in this case, always "WARNING").
        /// </summary>
        public override string? ErrorType { get; } = "WARNING";

        /// <summary>
        /// Represents the current AnalyzerWarning.
        /// </summary>
        /// <returns>Returns a string that represents the current AnalyzerWarning.</returns>
        public override string ToString()
        {
            return $"[{this.ErrorType}] ({this.ID}) {this.Message}";
        }
    }
}
