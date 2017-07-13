using System;

namespace Fiddle.Compilers
{
    public interface IDiagnostic
    {
        /// <summary>
        /// The Diagnostic's Message
        /// </summary>
        string Message { get; }
        /// <summary>
        /// The line the diagnostic is referring to
        /// </summary>
        int Line { get; }
        /// <summary>
        /// The column the diagnostic is referring to
        /// </summary>
        int Column { get; }
        /// <summary>
        /// The char position the diagnostic is referring to
        /// </summary>
        int Char { get; }

        /// <summary>
        /// Convert this <see cref="IDiagnostic"/> to an <see cref="Exception"/>
        /// </summary>
        /// <returns>The built <see cref="Exception"/></returns>
        Exception ToException();
    }
}