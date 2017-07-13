using Fiddle.Compilers.Implementation.CSharp;
using System;

namespace Fiddle.Compilers
{
    public enum Language { CSharp }

    /// <summary>
    /// A static host class for compiling and executing
    /// </summary>
    public static class Host
    {
        /// <summary>
        /// Create a new <see cref="ICompiler"/> with the given Language
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language"/> could not be found</exception>
        /// <returns>The initialized Compiler</returns>
        public static ICompiler NewCompiler(Language language)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpCompiler();
                default:
                    throw new LanguageNotFoundException(language);
            }
        }

        /// <summary>
        /// Compile source code directly
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The whole source code</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language"/> could not be found</exception>
        /// <returns>The execution result</returns>
        public static ICompileResult Compile(Language language, string code)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpCompiler().Compile;
                default:
                    throw new LanguageNotFoundException(language);
            }
        }

        /// <summary>
        /// Compile and Execute source code directly
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The whole source code</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language"/> could not be found</exception>
        /// <returns>The execution result</returns>
        public static IExecuteResult Execute(Language language, string code)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpCompiler().Execute();
                default:
                    throw new LanguageNotFoundException(language);
            }
        }
    }

    /// <summary>
    /// An exception that gets thrown when the language specified was not found
    /// </summary>
    public class LanguageNotFoundException : Exception
    {
        public LanguageNotFoundException(Language language) :
            base($"The language \"{language}\" could not be found!")
        { }
    }
}
