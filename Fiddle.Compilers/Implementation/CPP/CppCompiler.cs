using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.CPP {
    public class CppCompiler : ICompiler {
        public CppCompiler(string code, string[] imports = null) : this(code, new ExecutionProperties(),
            new CompilerProperties(), imports) { }

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
            IntPtr strPtr = Marshal.StringToHGlobalUni("source code goes here");
            string result = Marshal.PtrToStringAnsi(Compile(strPtr));
            Marshal.FreeHGlobal(strPtr);
            throw new NotImplementedException();
        }

        public Task<IExecuteResult> Execute() {
            IntPtr strPtr = Marshal.StringToHGlobalUni("assembly path goes here");
            string result = Marshal.PtrToStringAnsi(Execute(strPtr));
            Marshal.FreeHGlobal(strPtr);
            throw new NotImplementedException();
        }

        public void Dispose() {
            Provider?.Dispose();
        }


        [DllImport("ClangCompiler.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Compile")]
        private static extern IntPtr Compile(IntPtr sourcecode);

        [DllImport("ClangCompiler.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Execute")]
        private static extern IntPtr Execute(IntPtr filepath);
    }
}