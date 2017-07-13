using System;

namespace Fiddle.Compilers.Implementation.CSharp
{
    public class CSharpCompiler : ICompiler
    {
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }


        public ICompileResult Compile()
        {
            throw new NotImplementedException();
            CompileResult = null;
        }

        public IExecuteResult Execute()
        {
            if (CompileResult == default(ICompileResult))
            {
                Compile();
            }
            ExecuteResult = CompileResult.Execute();
            return ExecuteResult;
        }
    }
}
