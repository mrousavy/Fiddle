namespace Fiddle.Compilers
{
    /// <summary>
    /// A Code Compiler
    /// </summary>
    public interface ICompiler
    {
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
        ICompileResult Compile();

        /// <summary>
        /// Execute this compiled assembly
        /// </summary>
        /// <returns>The execution result</returns>
        IExecuteResult Execute();
    }
}
