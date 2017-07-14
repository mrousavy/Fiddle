using Fiddle.Compilers.Implementation.CPP;
using Fiddle.Compilers.Implementation.CSharp;
using Fiddle.Compilers.Implementation.Python;
using Fiddle.Compilers.Implementation.VB;
using System;
using System.Threading.Tasks;

namespace Fiddle.Compilers
{
    public enum Language { CSharp, Cpp, Vb, Python }

    /// <summary>
    /// A static host class for compiling and executing
    /// </summary>
    public static class Host
    {
        /// <summary>
        /// Create a new <see cref="ICompiler"/> with the given Language
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The source code</param>
        /// <param name="imports">All imports/namespaces that should be added to the script environment. 
        /// If this value is null, all references that can be found will be added (C#), or pre-defined 
        /// imports will be used (other languages)</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language"/> could not be found</exception>
        /// <returns>The initialized Compiler</returns>
        public static ICompiler NewCompiler(Language language, string code, string[] imports = null)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpCompiler(code);
                case Language.Cpp:
                    return new CppCompiler(code);
                case Language.Vb:
                    return new VbCompiler(code);
                case Language.Python:
                    return new PyCompiler(code);
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
        public static async Task<ICompileResult> Compile(Language language, string code)
        {
            switch (language)
            {
                case Language.CSharp:
                    return await new CSharpCompiler(code).Compile();
                case Language.Cpp:
                    return await new CppCompiler(code).Compile();
                case Language.Vb:
                    return await new VbCompiler(code).Compile();
                case Language.Python:
                    return await new PyCompiler(code).Compile();
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
        public static async Task<IExecuteResult> Execute(Language language, string code)
        {
            switch (language)
            {
                case Language.CSharp:
                    return await new CSharpCompiler(code).Execute();
                case Language.Cpp:
                    return await new CppCompiler(code).Execute();
                case Language.Vb:
                    return await new VbCompiler(code).Execute();
                case Language.Python:
                    return await new PyCompiler(code).Execute();
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
