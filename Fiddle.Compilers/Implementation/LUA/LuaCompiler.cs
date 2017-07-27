using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLua;
using NLua.Exceptions;

namespace Fiddle.Compilers.Implementation.LUA {
    public class LuaCompiler : ICompiler {
        public LuaCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public LuaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps, IGlobals globals = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            Globals = globals;
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
        public IGlobals Globals { get; }

        /// <summary>
        ///     Initialize Lua Script State (this function does not compile, LUA is a scripting language)
        /// </summary>
        /// <returns></returns>
        public Task<ICompileResult> Compile() {
            if (Lua == default(Lua)) {
                Lua = new Lua();
                Lua.RegisterFunction("print", this, typeof(LuaCompiler).GetMethod("Print")); //console out
                //Initialize Globals
                try { 
                if (Globals != null) {
                    foreach (PropertyInfo property in Globals.GetType().GetProperties()) {
                        Lua[property.Name] = property.GetValue(Globals);
                    }
                }
                } catch
                {
                    //reflection can cause many exceptions
                }
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
            if (Globals != null) {
                Globals.Console?.Dispose();
                Globals.Console = Writer;
            }

            ExecuteThreaded<object[]>.ThreadRunResult result = await ExecuteThreaded<object[]>.Execute(() =>
                Lua.DoString(SourceCode), (int) ExecuteProperties.Timeout);

            if(result.Exception is LuaScriptException scriptEx) {
                Regex number = new Regex("[0-9]+");
                Match match = number.Match(scriptEx.Message);
                if (match.Success) {
                    int num = int.Parse(match.Value);
                    List<IDiagnostic> diagnostics =
                        new List<IDiagnostic> { new LuaDiagnostic(scriptEx.Message, num, Severity.Error) };
                    CompileResult = new LuaCompileResult(result.ElapsedMilliseconds, SourceCode, diagnostics, diagnostics,
                        new List<Exception> {scriptEx});
                }
            }
            
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