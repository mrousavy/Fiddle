using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.VB {
    public class VbCompiler : ICompiler {
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public string[] Imports { get; set; }
        public Language Language { get; } = Language.Vb;

        private Script<object> Script { get; set; }

        public VbCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public VbCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps, string[] imports = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            if (imports == null)
                LoadReferences();
            else
                Imports = imports;

            Create();
        }

        /// <summary>
        /// Load all references/namespaces that can be used in the script environment
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
                .Where(a => Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(ra => ra.FullName).Contains(a.FullName))
                .Select(a => a.GetExportedTypes());

            //Add all types where namespace is not own namespace (no own references)
            foreach (Type[] type in types) {
                allNamespaces.AddRange(type
                    .Select(t => t.Namespace)
                    .Where(n => n != null && !ownNamespaces.Contains(n))
                    .Distinct());
            }

            Imports = allNamespaces.ToArray();
        }

        private void Create() {
            ScriptOptions options = ScriptOptions.Default
                .WithReferences(Imports)
                .WithImports(Imports);
            //TODO: VisualBasicScript
            Script = CSharpScript.Create(SourceCode, options, typeof(Globals));
        }

        public async Task<ICompileResult> Compile() {
            if (Script.Code != SourceCode) {
                Create();
            }

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
                bool graceful = compileThread.Join((int)CompilerProperties.Timeout); //Wait until compile Thread finishes with given timeout
                sw.Stop();

                if (!graceful)
                    throw new CompileException("The compilation timed out!");

                if (resultDiagnostics == null)
                    throw new CompileException("The compiler Thread was not returning any diagnostics!");

                //Pre-Enumerate so it's not enumerating multiple times
                IEnumerable<Diagnostic> diagnosticsEnum = resultDiagnostics as Diagnostic[] ?? resultDiagnostics.ToArray();
                IEnumerable<VbDiagnostic> diagnostics = diagnosticsEnum
                    .Select(d => new VbDiagnostic(
                        d.GetMessage(),
                        d.Location.GetLineSpan().StartLinePosition.Line,
                        d.Location.GetLineSpan().EndLinePosition.Line,
                        d.Location.GetLineSpan().StartLinePosition.Character,
                        d.Location.GetLineSpan().EndLinePosition.Character,
                        (Microsoft.Scripting.Severity)((int)d.Severity + 1)));

                IEnumerable<VbDiagnostic> warnings = diagnosticsEnum
                    .Where(d => d.Severity == DiagnosticSeverity.Warning)
                    .Select(d => new VbDiagnostic(
                        d.GetMessage(),
                        d.Location.GetLineSpan().StartLinePosition.Line,
                        d.Location.GetLineSpan().EndLinePosition.Line,
                        d.Location.GetLineSpan().StartLinePosition.Character,
                        d.Location.GetLineSpan().EndLinePosition.Character,
                        Microsoft.Scripting.Severity.Warning));
                IEnumerable<Exception> errors = diagnosticsEnum
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => new Exception(d.GetMessage()));

                //Build compile result object
                tcs.SetResult(new VbCompileResult(
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
            if (CompileResult == default(ICompileResult) || SourceCode != Script.Code) {
                await Compile();
            }
            if (!CompileResult.Success) {
                return new VbExecuteResult(-1, null, null, CompileResult, new Exception("The compilation was not successful!"));
            }

            StringBuilder builder = new StringBuilder();
            Globals globals = new Globals(builder);

            Stopwatch sw = Stopwatch.StartNew();
            ScriptState<object> state = await Script.RunAsync(globals, CatchException);
            sw.Stop();

            object returnValue = state.ReturnValue;
            string stdout = builder.ToString();

            IExecuteResult result = new VbExecuteResult(
                sw.ElapsedMilliseconds,
                stdout,
                returnValue,
                CompileResult,
                state.Exception);
            ExecuteResult = result;
            return result;
        }


        public bool CatchException(Exception ex) {
            return true;
        }
    }
}
