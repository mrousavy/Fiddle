using Fiddle.Compilers.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Java {
    public class JavaCompiler : ICompiler {
        public JavaCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public JavaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
        }

        public IExecutionProperties ExecuteProperties { get; set; }

        public ICompilerProperties CompilerProperties { get; set; }

        public string SourceCode { get; set; }

        public ICompileResult CompileResult { get; set; }

        public IExecuteResult ExecuteResult { get; set; }

        public Language Language { get; set; } = Language.Java;

        public async Task<ICompileResult> Compile() {
            Stopwatch sw = Stopwatch.StartNew();


            string output = null;
            Exception error = null;
            try {
                //TODO: Compile w/ Roslyn and import IKVM (clr-namespace:java)?
                java.lang.Class cl = java.lang.Class.forName("hello.HelloWorld");
            } catch (Exception ex) {
                error = ex;
            }

            sw.Stop();

            IEnumerable<IDiagnostic> diagnostics = null;
            IEnumerable<Exception> errors = null;

            if (output != null)
                diagnostics = new List<IDiagnostic> {
                    new JavaDiagnostic(output, -1, -1, -1, -1, Severity.Info)
                };
            if (error != null)
                errors = new List<Exception> {
                    error
                };

            JavaCompileResult result =
                new JavaCompileResult(sw.ElapsedMilliseconds, SourceCode, diagnostics, null, errors);
            CompileResult = result;
            return result;
        }

        public async Task<IExecuteResult> Execute() {
            if (CompileResult == null || CompileResult.SourceCode != SourceCode) await Compile();
            if (!CompileResult.Success) {
                JavaExecuteResult result = new JavaExecuteResult(0, "", null, CompileResult,
                    new CompileException("Could not compile, javac responded with some errors!"));
                ExecuteResult = result;
                return result;
            } else {
                Stopwatch sw = Stopwatch.StartNew();

                string output = null;
                Exception error = null;
                try {
                    //TODO: Execute w/ Roslyn and import IKVM (clr-namespace:java)?
                } catch (Exception ex) {
                    error = ex;
                }

                sw.Stop();

                JavaExecuteResult result =
                    new JavaExecuteResult(sw.ElapsedMilliseconds, output, null, CompileResult, error);
                ExecuteResult = result;
                return result;
            }
        }
    }
}
