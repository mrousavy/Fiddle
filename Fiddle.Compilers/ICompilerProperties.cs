namespace Fiddle.Compilers {
    public interface ICompilerProperties {
        /// <summary>
        ///     The maximum time the compilation can take
        ///     in milliseconds before being cancelled,
        ///     or -1 if no timeout is set
        /// </summary>
        long Timeout { get; }

        /// <summary>
        ///     The version of the language, or null if not specified
        /// </summary>
        string LanguageVersion { get; }
    }
}