using NLua;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.LUA {
    public class LuaCompiler : ICompiler {

        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public string[] Imports { get; set; }
        public Language Language { get; } = Language.Lua;
        private Lua Lua { get; set; }

        public LuaCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public LuaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
        }

        public Task<ICompileResult> Compile() {
            if (Lua == default(Lua))
                Lua = new Lua();

            ICompileResult compileResult = new LuaCompileResult(0, SourceCode, null, null, null);
            CompileResult = compileResult;
            return Task.FromResult(compileResult);
        }

        public Task<IExecuteResult> Execute() {
            if (Lua == default(Lua))
                Compile();

            Exception exception = null;
            object[] result = new object[0];

            Stopwatch sw = Stopwatch.StartNew();
            try {
                result = Lua.DoString(SourceCode);
            } catch (Exception ex) {
                exception = ex;
            }
            sw.Stop();

            IExecuteResult executeResult = new LuaExecuteResult(sw.ElapsedMilliseconds, result.ToString(), result, CompileResult, exception);
            ExecuteResult = executeResult;
            return Task.FromResult(executeResult);
        }
    }
}
