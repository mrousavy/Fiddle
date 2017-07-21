using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Fiddle.Compilers.Implementation.CSharp {
    public class CSharpCompiler : ICompiler {
        public CSharpCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public CSharpCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps,
            string[] imports = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            if (imports == null)
                LoadReferences();
            else
                Imports = imports;

            Create();
        }

        public string[] Imports { get; set; }

        private Script<object> Script { get; set; }
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public Language Language { get; } = Language.CSharp;

        public async Task<ICompileResult> Compile() {
            if (Script.Code != SourceCode)
                Create();

            TaskCompletionSource<ICompileResult> tcs = new TaskCompletionSource<ICompileResult>();

            //bad hack for creating an "async" method:
            new Thread(() => {
                //Init
                IEnumerable<Diagnostic> resultDiagnostics = null;

                //Actual compilation
                Stopwatch sw = Stopwatch.StartNew();
                Thread compileThread = new Thread(() => {
                    Compilation compilation = Script.GetCompilation();
                    resultDiagnostics = compilation.GetDiagnostics();
                });
                compileThread.Start();
                bool graceful =
                    compileThread.Join((int) CompilerProperties
                        .Timeout); //Wait until compile Thread finishes with given timeout
                sw.Stop();

                if (!graceful)
                    throw new CompileException("The compilation timed out!");

                if (resultDiagnostics == null)
                    throw new CompileException("The compiler Thread was not returning any diagnostics!");

                //Pre-Enumerate so it's not enumerating multiple times
                IEnumerable<Diagnostic> diagnosticsEnum =
                    resultDiagnostics as Diagnostic[] ?? resultDiagnostics.ToArray();
                IEnumerable<CSharpDiagnostic> diagnostics = diagnosticsEnum
                    .Select(d => new CSharpDiagnostic(
                        d.GetMessage(),
                        d.Location.GetLineSpan().StartLinePosition.Line + 1, //+1: it's 1-based
                        d.Location.GetLineSpan().EndLinePosition.Line + 1, //+1: it's 1-based
                        d.Location.GetLineSpan().StartLinePosition.Character + 1, //+1: it's 1-based
                        d.Location.GetLineSpan().EndLinePosition.Character + 1, //+1: it's 1-based
                        Host.ToSeverity(d.Severity)));

                IEnumerable<CSharpDiagnostic> warnings = diagnosticsEnum
                    .Where(d => d.Severity == DiagnosticSeverity.Warning)
                    .Select(d => new CSharpDiagnostic(
                        d.GetMessage(),
                        d.Location.GetLineSpan().StartLinePosition.Line + 1, //+1: it's 1-based
                        d.Location.GetLineSpan().EndLinePosition.Line + 1, //+1: it's 1-based
                        d.Location.GetLineSpan().StartLinePosition.Character + 1, //+1: it's 1-based
                        d.Location.GetLineSpan().EndLinePosition.Character + 1, //+1: it's 1-based
                        Host.ToSeverity(d.Severity)));
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

        public async Task<IExecuteResult> Execute() {
            if (CompileResult == default(ICompileResult) || SourceCode != Script.Code)
                await Compile();
            if (!CompileResult.Success)
                return new CSharpExecuteResult(-1, null, null, CompileResult,
                    new CompileException("The compilation was not successful!"));

            StringBuilder builder = new StringBuilder();
            Globals globals = new Globals(builder);
            int timeout = (int) ExecuteProperties.Timeout;

            ExecuteThreaded<ScriptState<object>>.ThreadRunResult threadRunResult =
                await ExecuteThreaded<ScriptState<object>>.Execute(() =>
                    Script.RunAsync(globals, CatchException), timeout);

            ScriptState<object> state = threadRunResult.ReturnValue;
            int elapsed = threadRunResult.ElapsedMilliseconds;

            IExecuteResult result;
            if (threadRunResult.Successful) {
                object returnValue = state.ReturnValue;
                string stdout = builder.ToString();
                result = new CSharpExecuteResult(
                    elapsed,
                    stdout,
                    returnValue,
                    CompileResult,
                    state.Exception);
            } else {
                result = new CSharpExecuteResult(
                    elapsed,
                    null,
                    null,
                    CompileResult,
                    new TimeoutException("The execution timed out! " +
                                         $"(Timeout: {timeout}ms)"));
            }
            ExecuteResult = result;
            return result;
        }

        public void Dispose() { }

        /// <summary>
        ///     Load all references/namespaces that can be used in the script environment
        /// </summary>
        public void LoadReferences() {
            //Get all namespaces from this assembly (own project, own library, ..)
            IEnumerable<string> ownNamespaces = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => t.Namespace)
                .Distinct();

            List<string> allNamespaces = new List<string>();

            //Get all .NET defined types
            IEnumerable<Type[]> types = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(ra => ra.FullName)
                    .Contains(a.FullName))
                .Select(a => a.GetExportedTypes());

            //Add all types where namespace is not own namespace (no own references)
            foreach (Type[] type in types)
                allNamespaces.AddRange(type
                    .Select(t => t.Namespace)
                    .Where(n => n != null && !ownNamespaces.Contains(n))
                    .Distinct());

            Imports = allNamespaces.ToArray();
        }

        private void Create() {
            ScriptOptions options = ScriptOptions.Default
                .WithReferences(Imports)
                .WithImports(Imports);
            Script = CSharpScript.Create(SourceCode, options, typeof(Globals));
        }


        public bool CatchException(Exception ex) {
            return true;
        }
    }
}