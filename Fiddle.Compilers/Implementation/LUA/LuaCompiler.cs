using System.IO;
using System.Threading.Tasks;
using NLua;

namespace Fiddle.Compilers.Implementation.LUA {
    public class LuaCompiler : ICompiler {
        public LuaCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public LuaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
        }

        public string[] Imports { get; set; }
        private Lua Lua { get; set; }
        public StringWriter Writer { get; set; }
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public Language Language { get; } = Language.Lua;

        /// <summary>
        ///     Initialize Lua Script State (this function does not compile, LUA is a scripting language)
        /// </summary>
        /// <returns></returns>
        public Task<ICompileResult> Compile() {
            if (Lua == default(Lua)) {
                Lua = new Lua();
                Lua.RegisterFunction("print", this, typeof(LuaCompiler).GetMethod("Print")); //console out
            }

            ICompileResult compileResult = new LuaCompileResult(0, SourceCode, null, null, null);
            CompileResult = compileResult;
            return Task.FromResult(compileResult);
        }

        public async Task<IExecuteResult> Execute() {
            if (Lua == default(Lua))
                await Compile();
            Writer?.Dispose();
            Writer = new StringWriter();

            ExecuteThreaded<object[]>.ThreadRunResult result = await ExecuteThreaded<object[]>.Execute(() =>
                Lua.DoString(SourceCode), (int) ExecuteProperties.Timeout);

            IExecuteResult executeResult =
                new LuaExecuteResult(
                    result.ElapsedMilliseconds,
                    Writer.ToString(),
                    result.ReturnValue,
                    CompileResult,
                    result.Exception);
            ExecuteResult = executeResult;
            return executeResult;
        }

        public void Dispose() {
            Lua?.Dispose();
        }

        public void Print(string message) {
            Writer.WriteLine(message);
        }
    }
}