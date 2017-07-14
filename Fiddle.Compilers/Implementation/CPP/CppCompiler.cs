using System;
using System.CodeDom.Compiler;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.CPP
{
    public class CppCompiler : ICompiler
    {
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public Language Language { get; } = Language.Cpp;

        private CodeDomProvider Provider { get; set; }

        public CppCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties())
        { }

        public CppCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps, string[] imports = null)
        {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;

            throw new NotImplementedException();
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
