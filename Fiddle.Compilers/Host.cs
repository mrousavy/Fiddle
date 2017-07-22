using Fiddle.Compilers.Implementation.CPP;
using Fiddle.Compilers.Implementation.CSharp;
using Fiddle.Compilers.Implementation.Java;
using Fiddle.Compilers.Implementation.LUA;
using Fiddle.Compilers.Implementation.Python;
using Fiddle.Compilers.Implementation.VB;
using Microsoft.CodeAnalysis;
using System;
using System.Threading.Tasks;

namespace Fiddle.Compilers {
    public enum Language {
        Cpp,
        CSharp,
        Java,
        Lua,
        Python,
        Vb
    }

    /// <summary>
    ///     A static host class for compiling and executing
    /// </summary>
    public static class Host {
        /// <summary>
        ///     Create a new <see cref="ICompiler" /> with the given Language
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The source code</param>
        /// <param name="imports">
        ///     All imports/namespaces that should be added to the script environment.
        ///     If this value is null, all references that can be found will be added (C#), or pre-defined
        ///     imports will be used (other languages)
        /// </param>
        /// <param name="jdkPath">Path to Java Development Kit</param>
        /// <param name="pySearchPath">Python libraries search path</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language" /> could not be found</exception>
        /// <returns>The initialized Compiler</returns>
        public static ICompiler NewCompiler(Language language, string code, string[] imports = null, string jdkPath = null, string pySearchPath = null) {
            switch (language) {
                case Language.CSharp:
                    return new CSharpCompiler(code, imports);
                case Language.Cpp:
                    return new CppCompiler(code, imports);
                case Language.Vb:
                    return new VbCompiler(code, imports);
                case Language.Python:
                    return new PyCompiler(code, pySearchPath);
                case Language.Java:
                    return new JavaCompiler(code, jdkPath);
                case Language.Lua:
                    return new LuaCompiler(code);
                default:
                    throw new LanguageNotFoundException(language);
            }
        }

        /// <summary>
        ///     Compile source code directly
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The whole source code</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language" /> could not be found</exception>
        /// <returns>The execution result</returns>
        public static async Task<ICompileResult> Compile(Language language, string code) {
            return await NewCompiler(language, code).Compile();
        }

        /// <summary>
        ///     Compile and Execute source code directly
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The whole source code</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language" /> could not be found</exception>
        /// <returns>The execution result</returns>
        public static async Task<IExecuteResult> Execute(Language language, string code) {
            return await NewCompiler(language, code).Execute();
        }

        /// <summary>
        ///     Convert severity enum
        /// </summary>
        internal static Severity ToSeverity(Microsoft.Scripting.Severity severity) {
            switch (severity) {
                case Microsoft.Scripting.Severity.Ignore:
                    return Severity.Info;
                case Microsoft.Scripting.Severity.Warning:
                    return Severity.Warning;
                case Microsoft.Scripting.Severity.Error:
                case Microsoft.Scripting.Severity.FatalError:
                    return Severity.Error;
                default:
                    throw new Exception("Severity not found!");
            }
        }

        /// <summary>
        ///     Convert severity enum
        /// </summary>
        internal static Severity ToSeverity(DiagnosticSeverity severity) {
            switch (severity) {
                case DiagnosticSeverity.Info:
                case DiagnosticSeverity.Hidden:
                    return Severity.Info;
                case DiagnosticSeverity.Warning:
                    return Severity.Warning;
                case DiagnosticSeverity.Error:
                    return Severity.Error;
                default:
                    throw new Exception("Severity not found!");
            }
        }
    }

    /// <summary>
    ///     An exception that gets thrown when the language specified was not found
    /// </summary>
    public class LanguageNotFoundException : Exception {
        public LanguageNotFoundException(Language language) :
            base($"The language \"{language}\" could not be found!") { }
    }
}