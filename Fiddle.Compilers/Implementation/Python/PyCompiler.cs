using System;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Python
{
    public class PyCompiler : ICompiler
    {
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; }
        public IExecuteResult ExecuteResult { get; }

        public PyCompiler(string code)
        {
            SourceCode = code;
        }


        public Task<ICompileResult> Compile()
        {

            throw new NotImplementedException();
        }

        public Task<IExecuteResult> Execute()
        {
            throw new NotImplementedException();
        }
    }
}
