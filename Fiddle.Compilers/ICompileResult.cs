using System;
using System.Collections.Generic;

namespace Fiddle.Compilers
{
    /// <summary>
    /// A result holding important information for a compilation
    /// </summary>
    public interface ICompileResult
    {
        /// <summary>
        /// The Time it took to compile the code in milliseconds
        /// </summary>
        long Time { get; }
        /// <summary>
        /// Indicates if the compilation was successful or not
        /// </summary>
        bool Success { get; }
        /// <summary>
        /// The Source Code of this Assembly
        /// </summary>
        string SourceCode { get; }
        /// <summary>
        /// All diagnostics from the compilation
        /// </summary>
        IEnumerable<IDiagnostic> Diagnostics { get; }
        /// <summary>
        /// All warnings from the diagnostics
        /// </summary>
        IEnumerable<IDiagnostic> Warnings { get; }
        /// <summary>
        /// All errors from the diagnostics
        /// </summary>
        IEnumerable<Exception> Errors { get; }

        /// <summary>
        /// Execute this Assembly
        /// </summary>
        /// <returns>The execution result</returns>
        IExecuteResult Execute();
    }
}