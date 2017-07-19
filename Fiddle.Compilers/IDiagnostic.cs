using System;

namespace Fiddle.Compilers {
    public enum Severity { Info, Warning, Error }

    public interface IDiagnostic {
        /// <summary>
        /// The Diagnostic's Message
        /// </summary>
        string Message { get; }
        /// <summary>
        /// The starting line the diagnostic is referring to
        /// </summary>
        int LineFrom { get; }
        /// <summary>
        /// The ending line the diagnostic is referring to
        /// </summary>
        int LineTo { get; }
        /// <summary>
        /// The beginning char position the diagnostic is referring to
        /// </summary>
        int CharFrom { get; }
        /// <summary>
        /// The ending char position the diagnostic is referring to
        /// </summary>
        int CharTo { get; }
        /// <summary>
        /// The Diagnostic's severity (Warning, Error, ..)
        /// </summary>
        Severity Severity { get; }

        /// <summary>
        /// Convert this <see cref="IDiagnostic"/> to an <see cref="Exception"/>
        /// </summary>
        /// <returns>The built <see cref="Exception"/></returns>
        Exception ToException();
    }
}