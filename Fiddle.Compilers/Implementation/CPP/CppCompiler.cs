using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.CPP {
    public class CppCompiler : ICompiler {
        public CppCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public CppCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps,
            string[] imports = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;

        }

        private CodeDomProvider Provider { get; set; }
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public Language Language { get; } = Language.Cpp;

        public Task<ICompileResult> Compile() {
            string result = Compile("test");
            throw new NotImplementedException();
        }

        public Task<IExecuteResult> Execute() {
            string result = Execute("test");
            return null;
        }

        public void Dispose() {
            Provider?.Dispose();
        }


        [DllImport("ClangCompiler.dll", EntryPoint = "Compile")]
        private static extern string Compile(string sourcecode);

        [DllImport("ClangCompiler.dll", EntryPoint = "Execute")]
        private static extern string Execute(string filepath);
    }
}