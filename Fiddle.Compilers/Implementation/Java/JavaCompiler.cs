using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Java {
    public class JavaCompiler : ICompiler {
        public JavaCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public JavaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            FindJdk();
            FindJre();
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
                    output = await JreHelper.ExecuteJava(JrePath, ClassName, ExecuteProperties);
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

        private void FindJdk() {
            string environmentVariable = Environment.GetEnvironmentVariable("path");
            if (!string.IsNullOrWhiteSpace(environmentVariable)) {
                string[] path = environmentVariable.Split(';');
                string jdk = path.FirstOrDefault(p => p.Contains("jdk"));
                if (!string.IsNullOrWhiteSpace(jdk)) {
                    JdkPath = Path.Combine(jdk, "javac.exe");
                    return;
                }
            }
            string jdkHome = Environment.GetEnvironmentVariable("JDK_HOME");
            if (!string.IsNullOrWhiteSpace(jdkHome)) {
                JdkPath = jdkHome;
                return;
            }
            string java86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java");
            string javac86 = JdkHelper.SearchJavac(java86);
            if (!string.IsNullOrWhiteSpace(javac86)) {
                JdkPath = javac86;
                return;
            }
            string java = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java");
            string javac = JdkHelper.SearchJavac(java);
            if (!string.IsNullOrWhiteSpace(javac)) {
                JdkPath = javac;
                return;
            }

            throw new CompileException("Java Development Kit (JDK) could not be found on this System!");
        }

        private void FindJre() {
            string environmentVariable = Environment.GetEnvironmentVariable("path");
            if (!string.IsNullOrWhiteSpace(environmentVariable)) {
                string[] path = environmentVariable.Split(';');
                string jdk = path.FirstOrDefault(p => p.Contains("jdk"));
                if (!string.IsNullOrWhiteSpace(jdk)) {
                    JrePath = Path.Combine(jdk, "java.exe");
                    return;
                }
                string javapath = path.FirstOrDefault(p => p.Contains("javapath"));
                if (!string.IsNullOrWhiteSpace(javapath)) {
                    JrePath = Path.Combine(javapath, "java.exe");
                    return;
                }
            }
            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrWhiteSpace(javaHome)) {
                JrePath = Path.Combine(javaHome, "java.exe");
                return;
            }

            string java86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java");
            string javac86 = JdkHelper.SearchJava(java86);
            if (!string.IsNullOrWhiteSpace(javac86)) {
                JrePath = javac86;
                return;
            }
            string java = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java");
            string javac = JdkHelper.SearchJava(java);
            if (!string.IsNullOrWhiteSpace(javac)) {
                JrePath = javac;
                return;
            }

            throw new CompileException("Java Runtime Environment (JRE) could not be found on this System!");
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