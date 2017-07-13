using System.Threading.Tasks;

namespace Fiddle.Compilers
{
    /// <summary>
    /// A Code Compiler
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// The execution properties to respect when executing
        /// </summary>
        IExecutionProperties ExecuteProperties { get; }
        /// <summary>
        /// The compiler properties to respect when compiling
        /// </summary>
        ICompilerProperties CompilerProperties { get; }
        /// <summary>
        /// The source code for this compiler
        /// </summary>
        string SourceCode { get; }
        /// <summary>
        /// The result returned by <see cref="Compile()"/>, or null if not yet compiled
        /// </summary>
        ICompileResult CompileResult { get; }
        /// <summary>
        /// The result returned by <see cref="Execute()"/>, or null if not yet compiled
        /// </summary>
        IExecuteResult ExecuteResult { get; }


        /// <summary>
        /// Compile this <see cref="ICompiler"/> instance
        /// </summary>
        /// <returns>The execution result</returns>
        Task<ICompileResult> Compile();

        /// <summary>
        /// Execute this compiled assembly
        /// </summary>
        /// <returns>The execution result</returns>
        Task<IExecuteResult> Execute();
    }
}
