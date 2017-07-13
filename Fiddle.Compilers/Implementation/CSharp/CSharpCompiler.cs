using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.CSharp
{
    public class CSharpCompiler : ICompiler
    {
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }

        private Script<object> Script { get; set; }

        public CSharpCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties())
        {
        }

        public CSharpCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps)
        {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            Create();
        }

        private void Create()
        {
            ScriptOptions options = ScriptOptions.Default;
            Script = CSharpScript.Create(SourceCode, options, typeof(Globals));
        }

        public async Task<ICompileResult> Compile()
        {
            if (Script.Code != SourceCode)
            {
                Create();
            }

            TaskCompletionSource<ICompileResult> tcs = new TaskCompletionSource<ICompileResult>();

            //bad hack for creating an "async" method:
            new Thread(() =>
            {
                //Init
                IEnumerable<Diagnostic> resultDiagnostics = null;

                //Actual compilation
                Stopwatch sw = Stopwatch.StartNew();
                Thread compileThread = new Thread(() =>
                {
                    Compilation compilation = Script.GetCompilation();
                    resultDiagnostics = compilation.GetDiagnostics();
                });
                compileThread.Start();
                compileThread.Join((int)CompilerProperties.Timeout); //Wait until compile Thread finishes with given timeout
                sw.Stop();

                if (resultDiagnostics == null)
                    throw new CompileException("The compiler Thread was not returning any diagnostics!");

                //Pre-Enumerate so it's not enumerating multiple times
                IEnumerable<Diagnostic> diagnosticsEnum = resultDiagnostics as Diagnostic[] ?? resultDiagnostics.ToArray();
                IEnumerable<CSharpDiagnostic> diagnostics = diagnosticsEnum
                    .Select(d => new CSharpDiagnostic(
                        d.GetMessage(),
                        d.Location.GetLineSpan().StartLinePosition.Line,
                        d.Location.GetLineSpan().StartLinePosition.Character,
                        d.Location.GetLineSpan().StartLinePosition.Character));
                IEnumerable<CSharpDiagnostic> warnings = diagnosticsEnum
                    .Where(d => d.Severity == DiagnosticSeverity.Warning)
                    .Select(d => new CSharpDiagnostic(
                        d.GetMessage(),
                        d.Location.GetLineSpan().StartLinePosition.Line,
                        d.Location.GetLineSpan().StartLinePosition.Character,
                        d.Location.GetLineSpan().StartLinePosition.Character));
                IEnumerable<Exception> errors = diagnosticsEnum
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => new Exception(d.GetMessage()));

                //Build compile result object
                tcs.SetResult(new CSharpCompileResult(
                        sw.ElapsedMilliseconds,
                        SourceCode,
                        Script,
                        diagnostics,
                        warnings,
                        errors));
            }).Start();

            ICompileResult compileResult = await tcs.Task;
            CompileResult = compileResult;
            return compileResult;
        }

        public async Task<IExecuteResult> Execute()
        {
            if (CompileResult == default(ICompileResult))
            {
                await Compile();
            }
            StringBuilder builder = new StringBuilder();
            Globals globals = new Globals(builder);

            Stopwatch sw = Stopwatch.StartNew();
            ScriptState<object> state = await Script.RunAsync(globals, CatchException);
            sw.Stop();

            object returnValue = state.ReturnValue;
            string stdout = builder.ToString();

            IExecuteResult result = new CSharpExecuteResult(sw.ElapsedMilliseconds, true, stdout, returnValue, CompileResult);
            return result;
        }


        public bool CatchException(Exception ex)
        {
            return true;
        }
    }
}
