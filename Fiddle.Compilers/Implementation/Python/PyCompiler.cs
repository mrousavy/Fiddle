using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Python
{
    public class PyCompiler : ICompiler
    {
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }

        private ScriptScope Scope { get; set; }
        private CompiledCode Compilation { get; set; }
        private ScriptSource Source { get; set; }

        public PyCompiler(string code) : this(code, new CompilerProperties(), new ExecutionProperties()) { }

        public PyCompiler(string code, ICompilerProperties cProps, IExecutionProperties eProps)
        {
            SourceCode = code;
            CompilerProperties = cProps;
            ExecuteProperties = eProps;
        }


        public async Task<ICompileResult> Compile()
        {
            TaskCompletionSource<ICompileResult> tcs = new TaskCompletionSource<ICompileResult>();

            new Thread(() =>
            {
                PyErrorListener listener = new PyErrorListener();

                //Start the stopwatch
                Stopwatch sw = Stopwatch.StartNew();
                //Spawn a new thread with the compile process
                Thread compileThread = new Thread(() =>
                    {
                        ScriptEngine engine = IronPython.Hosting.Python.CreateEngine();
                        Scope = engine.CreateScope();
                        Source = engine.CreateScriptSourceFromString(SourceCode);
                        Compilation = Source.Compile(listener);
                    });
                compileThread.Start();
                //Join the thread into main thread with specified timeout
                bool graceful = compileThread.Join((int)CompilerProperties.Timeout);
                sw.Stop();

                if (!graceful)
                    listener.Diagnostics.Add(new PyDiagnostic("Compilation timed out!", 0, 0, Severity.Error));

                tcs.SetResult(new PyCompileResult(sw.ElapsedMilliseconds, SourceCode, listener.Diagnostics));
            }).Start();

            ICompileResult result = await tcs.Task;
            CompileResult = result;
            return result;
        }

        public async Task<IExecuteResult> Execute()
        {
            if (Compilation == null || SourceCode != Source.GetCode())
                await Compile();
            if (!CompileResult.Success)
                return new PyExecuteResult(-1, null, null, CompileResult,
                    new Exception("Compilation was not successful!"));

            TaskCompletionSource<dynamic> tcs = new TaskCompletionSource<dynamic>();

            Stopwatch sw = Stopwatch.StartNew();
            //Cheap hack: Spawn new thread for executing to allow async/await
            new Thread(() =>
            {
                try
                {
                    dynamic retValue = Compilation.Execute(Scope);
                    tcs.SetResult(retValue);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }).Start();
            object result = await tcs.Task;
            sw.Stop();

            if (result != null && result.GetType() == typeof(Exception))
            {
                IExecuteResult execResultInner = new PyExecuteResult(-1, null, null, CompileResult,
                    result as Exception);
                ExecuteResult = execResultInner;
                return execResultInner;
            }

            IExecuteResult execResult = new PyExecuteResult(sw.ElapsedMilliseconds, result?.ToString(), result, CompileResult, null);
            ExecuteResult = execResult;
            return execResult;
        }


        public class PyErrorListener : ErrorListener
        {
            public IList<IDiagnostic> Diagnostics { get; set; }

            public PyErrorListener()
            {
                Diagnostics = new List<IDiagnostic>();
            }

            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity)
            {
                Diagnostics.Add(new PyDiagnostic(message, span.Start.Line, span.Start.Column, severity));
            }
        }
    }
}
