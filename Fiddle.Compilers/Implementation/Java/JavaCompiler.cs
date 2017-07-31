using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string JrePath { get; set; }
        private string ClassName { get; set; }
        public IExecutionProperties ExecuteProperties { get; set; }

        public ICompilerProperties CompilerProperties { get; set; }

        public string SourceCode { get; set; }

        public ICompileResult CompileResult { get; set; }

        public IExecuteResult ExecuteResult { get; set; }

        public Language Language { get; set; } = Language.Java;

        public async Task<ICompileResult> Compile() {
            Stopwatch sw = Stopwatch.StartNew();
            ToValidCode();

            string tmp = Path.Combine(Path.GetTempPath(), $"{ClassName}.java");
            File.WriteAllText(tmp, SourceCode);

            string output = null;
            Exception error = null;
            try {
                output = await JdkHelper.CompileJava(JdkPath, tmp, CompilerProperties);
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
                    output = JreHelper.ExecuteJava(JrePath, ClassName, ExecuteProperties);
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
                    JdkPath = Path.Combine(jdk, "bin", "javac.exe");
                    JrePath = Path.Combine(jdk, "bin", "java.exe");
                    return;
                }
            }
            string javaPath86 = Path.Combine(programFilesX86, "Java");
            Tuple<string, string> exesx86 = JdkHelper.SearchJavaExecutables(javaPath86);
            if (exesx86 != null) {
                JdkPath = exesx86.Item1;
                JrePath = exesx86.Item2;
                return;
            }
            string javaPath = Path.Combine(programFiles, "Java");
            Tuple<string, string> exes = JdkHelper.SearchJavaExecutables(javaPath);
            if (exes != null) {
                JdkPath = exes.Item1;
                JrePath = exes.Item2;
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