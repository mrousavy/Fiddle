using System;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.CSharp
{
    public class CSharpCompiler : ICompiler
    {
        public string SourceCode { get; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }


        public CSharpCompiler(string code)
        {
            SourceCode = code;
        }

        public async Task<ICompileResult> Compile()
        {
            throw new NotImplementedException();
            CompileResult = null;
        }

        public async Task<IExecuteResult> Execute()
        {
            if (CompileResult == default(ICompileResult))
            {
                Compile();
            }
            ExecuteResult = await CompileResult.Execute();
            return ExecuteResult;
        }
    }
}
