using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Fiddle.Compilers.Implementation.CSharp {
    public class CSharpCompiler : ICompiler {
        public CSharpCompiler(string code, string[] imports = null) : this(code, new ExecutionProperties(),
            new CompilerProperties(), imports) { }

        public CSharpCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps,
            string[] imports = null, IGlobals globals = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            if (imports == null || !imports.Any())
                LoadReferences();
            else
                Imports = imports;

            Globals = globals ?? new Globals(new StringBuilder());
            Create();
        }

        public string[] Imports { get; set; }

        private Script<object> Script { get; set; }
        public IGlobals Globals { get; }
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public Language Language { get; } = Language.CSharp;

        public async Task<ICompileResult> Compile() {
            if (Script.Code != SourceCode)
                Create();

            var runResult =
                await ExecuteThreaded<ImmutableArray<Diagnostic>>.Execute(
                    () => {
                        //Actual compilation
                        var compilation = Script.GetCompilation();
                        return compilation.GetDiagnostics();
                    }, (int) CompilerProperties.Timeout
                );

            if (!runResult.Successful)
                throw new CompileException("The compilation timed out!");
            if (runResult.ReturnValue == null)
                throw new CompileException("The compiler Thread was not returning any diagnostics!");

            ImmutableArray<Diagnostic> resultDiagnostics = runResult.ReturnValue;

            //Pre-Enumerate so it's not enumerating multiple times
            IEnumerable<CSharpDiagnostic> diagnostics = resultDiagnostics
                .Select(d => new CSharpDiagnostic(
                    d.GetMessage(),
                    d.Location.GetLineSpan().StartLinePosition.Line + 1, //+1: it's 1-based
                    d.Location.GetLineSpan().EndLinePosition.Line + 1, //+1: it's 1-based
                    d.Location.GetLineSpan().StartLinePosition.Character + 1, //+1: it's 1-based
                    d.Location.GetLineSpan().EndLinePosition.Character + 1, //+1: it's 1-based
                    Host.ToSeverity(d.Severity)));

            IEnumerable<CSharpDiagnostic> warnings = resultDiagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Warning)
                .Select(d => new CSharpDiagnostic(
                    d.GetMessage(),
                    d.Location.GetLineSpan().StartLinePosition.Line + 1, //+1: it's 1-based
                    d.Location.GetLineSpan().EndLinePosition.Line + 1, //+1: it's 1-based
                    d.Location.GetLineSpan().StartLinePosition.Character + 1, //+1: it's 1-based
                    d.Location.GetLineSpan().EndLinePosition.Character + 1, //+1: it's 1-based
                    Host.ToSeverity(d.Severity)));
            IEnumerable<Exception> errors = resultDiagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => new Exception(d.GetMessage()));

            //Build compile result object
            CompileResult = new CSharpCompileResult(
                runResult.ElapsedMilliseconds,
                SourceCode,
                Script,
                diagnostics,
                warnings,
                errors);
            return CompileResult;
        }

        public async Task<IExecuteResult> Execute() {
            if (CompileResult == default(ICompileResult) || SourceCode != Script.Code)
                await Compile();
            if (!CompileResult.Success)
                return new CSharpExecuteResult(-1, null, null, CompileResult,
                    new CompileException("The compilation was not successful!"));

            //Reset builder/Clear console
            var builder = new StringBuilder();
            Globals.Console?.Dispose();
            Globals.Console = new StringWriter(builder);
            int timeout = (int) ExecuteProperties.Timeout;

            var threadRunResult =
                await ExecuteThreaded<ScriptState<object>>.Execute(() =>
                    Script.RunAsync(Globals, CatchException), timeout);

            ScriptState<object> state = threadRunResult.ReturnValue;
            int elapsed = threadRunResult.ElapsedMilliseconds;

            IExecuteResult result = threadRunResult.Successful
                ? new CSharpExecuteResult(
                    elapsed,
                    builder.ToString(),
                    state.ReturnValue,
                    CompileResult,
                    state.Exception)
                : new CSharpExecuteResult(
                    elapsed,
                    null,
                    null,
                    CompileResult,
                    new TimeoutException($"The execution timed out! (Timeout: {timeout}ms)"));
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
            var options = ScriptOptions.Default
                .WithReferences(Imports)
                .WithImports(Imports);
            Script = CSharpScript.Create(SourceCode, options, Globals.GetType());
        }


        public bool CatchException(Exception ex) {
            return true;
        }
    }
}