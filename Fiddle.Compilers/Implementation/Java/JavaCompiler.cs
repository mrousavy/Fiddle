using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Java {
    public class JavaCompiler : ICompiler {
        public JavaCompiler(string code, string jdkPath = null) : this(code, new ExecutionProperties(),
            new CompilerProperties(), jdkPath) { }

        public JavaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps,
            string jdkPath = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;

            if (string.IsNullOrWhiteSpace(jdkPath) || !Directory.Exists(jdkPath))
                //Search for JDK if jdkPath is invalid directory
                FindJdk();
            else
                //Use JDK Path parameter
                JdkPath = jdkPath;
        }

        private string JdkPath { get; set; }
        private string JavacPath => Path.Combine(JdkPath, "bin", "javac.exe");
        private string JavaPath => Path.Combine(JdkPath, "bin", "java.exe");
        private string ClassName { get; set; }
        public IExecutionProperties ExecuteProperties { get; set; }

        public ICompilerProperties CompilerProperties { get; set; }

        public string SourceCode { get; set; }

        public ICompileResult CompileResult { get; set; }

        public IExecuteResult ExecuteResult { get; set; }

        public Language Language { get; set; } = Language.Java;

        public async Task<ICompileResult> Compile() {
            ToValidCode();

            string tmp = Path.Combine(Path.GetTempPath(), $"{ClassName}.java");
            File.WriteAllText(tmp, SourceCode);
            
            var runResult = await ExecuteThreaded<string>.Execute(
                () => JdkHelper.CompileJava(JavacPath, tmp, CompilerProperties), (int)CompilerProperties.Timeout
            );
            string output = runResult.ReturnValue;
            Exception error = runResult.Exception;
            int time = runResult.ElapsedMilliseconds;

            IEnumerable<IDiagnostic> diagnostics = null;
            IEnumerable<Exception> errors = null;

            if (!string.IsNullOrWhiteSpace(output))
                diagnostics = new List<IDiagnostic> {
                    new JavaDiagnostic(output, -1, -1, -1, -1, Severity.Info)
                };
            if (error != null)
                errors = new List<Exception> { error };

            JavaCompileResult result =
                new JavaCompileResult(time, SourceCode, diagnostics, null, errors);
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
                var runResult = await ExecuteThreaded<string>.Execute(
                    () => JreHelper.ExecuteJava(JavaPath, ClassName, ExecuteProperties), (int)ExecuteProperties.Timeout
                );
                string output = runResult.ReturnValue;
                int time = runResult.ElapsedMilliseconds;
                Exception error = runResult.Exception;

                JavaExecuteResult result =
                    new JavaExecuteResult(time, output, null, CompileResult, error);
                ExecuteResult = result;
                return result;
            }
        }

        public void Dispose() { }

        private void FindJdk() {
            string programFiles;
            string programFilesX86;
            if (Environment.Is64BitProcess) {
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            } else {
                programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
                programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
            }

            string environmentVariable = Environment.GetEnvironmentVariable("path");
            if (!string.IsNullOrWhiteSpace(environmentVariable)) {
                string[] path = environmentVariable.Split(';');
                string jdk = path.FirstOrDefault(p => p.Contains("jdk"));
                if (!string.IsNullOrWhiteSpace(jdk)) {
                    JdkPath = jdk;
                    return;
                }
            }
            string javaPath86 = Path.Combine(programFilesX86, "Java");
            string java = JdkHelper.SearchJavaPath(javaPath86);
            if (java != null) {
                JdkPath = java;
                return;
            }
            string javaPath = Path.Combine(programFiles, "Java");
            java = JdkHelper.SearchJavaPath(javaPath);
            if (java != null) {
                JdkPath = java;
                return;
            }

            throw new CompileException("Java Development Kit (JDK) could not be found on this System!");
        }

        private void ToValidCode() {
            ToValidMain();
            ToValidClass();
        }

        private void ToValidClass() {
            Regex findClass = new Regex("class ([A-Za-z]+)");
            Match match = findClass.Match(SourceCode);
            if (match.Success) {
                string matchString = SourceCode.Substring(match.Index, match.Length);
                ClassName = matchString.Split(' ')[1]; //split "class Test" -> ["class", "Test"] and pick [1]: "Test"
            } else {
                ClassName = "FiddleClass";
                SourceCode = $"public class {ClassName} {{\n" +
                             $"{SourceCode}\n" +
                             "}";
            }
        }

        private void ToValidMain() {
            if (!SourceCode.Contains("static void main"))
                SourceCode = "public static void main(String[] args) {\n" +
                             $"{SourceCode}\n" +
                             "}";
        }
    }
}