using Fiddle.Compilers.Implementation.CPP;
using Fiddle.Compilers.Implementation.CSharp;
using Fiddle.Compilers.Implementation.Java;
using Fiddle.Compilers.Implementation.Python;
using Fiddle.Compilers.Implementation.VB;
using Microsoft.CodeAnalysis;
using System;
using System.Threading.Tasks;
using Fiddle.Compilers.Implementation.LUA;

namespace Fiddle.Compilers {
    public enum Language {
        /// <summary>
        /// C++
        /// </summary>
        Cpp,
        /// <summary>
        /// C# .NET
        /// </summary>
        CSharp,
        /// <summary>
        /// Java
        /// </summary>
        Java,
        /// <summary>
        /// NLUA (KeraLua)
        /// </summary>
        Lua,
        /// <summary>
        /// IronPython
        /// </summary>
        Python,
        /// <summary>
        /// Visual Basic .NET
        /// </summary>
        Vb
    }

    /// <summary>
    /// A set of properties for compiler creation
    /// </summary>
    public struct Properties {
        /// <summary>
        /// The language to use for compilation and execution
        /// </summary>
        public Language Language { get; set; }
        /// <summary>
        /// The source code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// All imports/namespaces that should be added to the script environment.
        /// If this value is null, all references that can be found will be added (C#), or pre-defined
        /// imports will be used (other languages)
        /// </summary>
        public string[] Imports { get; set; }
        /// <summary>
        /// Path to Java Development Kit
        /// </summary>
        public string JdkPath { get; set; }
        /// <summary>
        /// Python libraries search path
        /// </summary>
        public string PySearchPath { get; set; }
        /// <summary>
        /// Given Execution Properties for the compiler (Use <see cref="Fiddle.Compilers.Implementation.ExecutionProperties"/>)
        /// </summary>
        public IExecutionProperties ExecuteProperties { get; set; }
        /// <summary>
        /// Given Compilation Properties for the compiler (Use <see cref="Fiddle.Compilers.Implementation.CompilerProperties"/>)
        /// </summary>
        public ICompilerProperties CompilerProperties { get; set; }
        /// <summary>
        /// Global variables to be used in a language (e.g. C#) (e.g. for redirecting Console to a StreamWriter)
        /// </summary>
        public IGlobals Globals { get; set; }

        public Properties(Language language, string code, 
            string[] imports = null,
            string jdkPath = null,
            string pyPath = null,
            IExecutionProperties exProps = null,
            ICompilerProperties comProps = null,
            IGlobals globals = null) {
            Language = language;
            Code = code;
            Imports = imports;
            JdkPath = jdkPath;
            PySearchPath = pyPath;
            ExecuteProperties = exProps;
            CompilerProperties = comProps;
            Globals = globals;
        }
    }

    /// <summary>
    ///     A static host class for compiling and executing
    /// </summary>
    public static class Host {
        /// <summary>
        ///     Create a new <see cref="ICompiler" /> with the given Language
        /// </summary>
        /// <param name="properties">Given Compilation and Execution Properties for the compiler creation</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language" /> could not be found</exception>
        /// <returns>The initialized Compiler</returns>
        public static ICompiler NewCompiler(Properties properties) {
            string code = properties.Code;
            IExecutionProperties exProps = properties.ExecuteProperties;
            ICompilerProperties comProps = properties.CompilerProperties;
            string[] imports = properties.Imports;
            string pySearchPath = properties.PySearchPath;
            string jdkPath = properties.JdkPath;
            Language language = properties.Language;
            IGlobals globals = properties.Globals;

            switch (language) {
                case Language.CSharp:
                    return new CSharpCompiler(code, exProps, comProps, imports, globals);
                case Language.Cpp:
                    return new CppCompiler(code, exProps, comProps, imports);
                case Language.Vb:
                    return new VbCompiler(code, exProps, comProps, imports);
                case Language.Python:
                    return new PyCompiler(code, exProps, comProps, pySearchPath);
                case Language.Java:
                    return new JavaCompiler(code, exProps, comProps, jdkPath);
                case Language.Lua:
                    return new LuaCompiler(code, exProps, comProps, globals);
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
            return await NewCompiler(new Properties { Language = language, Code = code }).Compile();
        }

        /// <summary>
        ///     Compile and Execute source code directly
        /// </summary>
        /// <param name="language">The language to use for compilation and execution</param>
        /// <param name="code">The whole source code</param>
        /// <exception cref="LanguageNotFoundException">When the given <see cref="Language" /> could not be found</exception>
        /// <returns>The execution result</returns>
        public static async Task<IExecuteResult> Execute(Language language, string code) {
            return await NewCompiler(new Properties{Language = language, Code = code}).Execute();
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