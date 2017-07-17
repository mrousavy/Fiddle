using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Java {
    public class JavaCompiler : ICompiler {
        public IExecutionProperties ExecuteProperties { get; set; }

        public ICompilerProperties CompilerProperties { get; set; }

        public string SourceCode { get; set; }

        public ICompileResult CompileResult { get; set; }

        public IExecuteResult ExecuteResult { get; set; }

        public Language Language { get; set; }

        private string JdkPath { get; set; }
        private string JrePath { get; set; }
        private string ClassName { get; set; }
        private string ByteCodePath { get; set; }


        public JavaCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public JavaCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps, string[] imports = null) {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            FindJdk();
            FindJre();
        }

        private void FindJdk() {
            string[] path = Environment.GetEnvironmentVariable("path").Split(';');
            if (path.FirstOrDefault(p => p.Contains("jdk")) is string jdk) {
                JdkPath = Path.Combine(jdk, "javac.exe");
                return;
            } else if (Environment.GetEnvironmentVariable("JDK_HOME") is string jdkHome) {
                JdkPath = jdkHome;
                return;
            } else {
                string java86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java");
                if (JdkHelper.SearchJavac(java86) is string javac86) {
                    JdkPath = javac86;
                    return;
                }
                string java = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java");
                if (JdkHelper.SearchJavac(java) is string javac) {
                    JdkPath = javac;
                    return;
                }
            }

            throw new CompileException("Java Development Kit (JDK) could not be found on this System!");
        }

        private void FindJre() {
            string[] path = Environment.GetEnvironmentVariable("path").Split(';');
            if (path.FirstOrDefault(p => p.Contains("jdk")) is string jdk) {
                JrePath = Path.Combine(jdk, "java.exe");
                return;
            } else if (path.FirstOrDefault(p => p.Contains("javapath")) is string javapath) {
                JrePath = Path.Combine(javapath, "java.exe");
                return;
            } else if (Environment.GetEnvironmentVariable("JAVA_HOME") is string javaHome) {
                JrePath = Path.Combine(javaHome, "java.exe");
                return;
            } else {
                string java86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java");
                if (JdkHelper.SearchJava(java86) is string javac86) {
                    JrePath = javac86;
                    return;
                }
                string java = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java");
                if (JdkHelper.SearchJava(java) is string javac) {
                    JrePath = javac;
                    return;
                }
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
            if (!SourceCode.Contains("static void main")) {
                SourceCode = "public static void main(String[] args) {\n" +
                                $"{SourceCode}\n" +
                            "}";
            }
        }

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

            if (output != null) {
                diagnostics = new List<IDiagnostic> {
                    new JavaDiagnostic(output, -1, -1, -1, -1, Microsoft.Scripting.Severity.Ignore)
                };
            }
            if (error != null) {
                errors = new List<Exception> {
                    error
                };
            }

            JavaCompileResult result = new JavaCompileResult(sw.ElapsedMilliseconds, SourceCode, diagnostics, null, errors);
            if (result.Success) {
                ByteCodePath = Path.Combine(Path.GetTempPath(), $"{ClassName}.class");
            }
            CompileResult = result;
            return result;
        }

        public async Task<IExecuteResult> Execute() {
            if (CompileResult == null || CompileResult.SourceCode != SourceCode) {
                await Compile();
            }
            if (!CompileResult.Success) {
                throw new CompileException("Could not compile, javac responded with some errors!");
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

                JavaExecuteResult result = new JavaExecuteResult(sw.ElapsedMilliseconds, output, null, CompileResult, error);
                ExecuteResult = result;
                return result;
            }
        }
    }
}
