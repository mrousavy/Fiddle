using System;

namespace Fiddle.Compilers {
    /// <summary>
    ///     A result from an execution
    /// </summary>
    public interface IExecuteResult {
        /// <summary>
        ///     The Time it took to execute the code in milliseconds
        /// </summary>
        long Time { get; }

        /// <summary>
        ///     Indicates if the execution was successful or not
        /// </summary>
        bool Success { get; }

        /// <summary>
        ///     The returned value or null if none. (if this value
        ///     cannot be casted to a managed object, this is a string)
        /// </summary>
        object ReturnValue { get; }

        /// <summary>
        ///     The output of the default Console (stdout)
        ///     or null if none
        /// </summary>
        string ConsoleOutput { get; }

        /// <summary>
        ///     The assembly's compile result
        /// </summary>
        ICompileResult CompileResult { get; }

        /// <summary>
        ///     The thrown exception, if <see cref="Success" /> then this field is null
        /// </summary>
        Exception Exception { get; }
        /// <summary>
        /// The Line Number of the Exception that occured (or -1 if none)
        /// </summary>
        int ExceptionLineNr { get; }
    }
}