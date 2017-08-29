using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Java {
    public class JdkHelper {
        /// <summary>
        ///     Load javac.exe and run compilation from arguments
        /// </summary>
        /// <param name="javacPathName">Path to javac.exe (Program Files\java\jdk\bin\javac.exe)</param>
        /// <param name="javaFilePathName">Source code file name</param>
        /// <param name="properties">Compilation properties</param>
        /// <param name="commandLineOptions">Any compiler options</param>
        public static async Task<string> CompileJava(string javacPathName, string javaFilePathName,
            ICompilerProperties properties, string commandLineOptions = "") {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = javacPathName,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = commandLineOptions + " " + javaFilePathName
            };
            using (Process javacProcess = Process.Start(startInfo)) {
                if(javacProcess == null)
                    throw new CompileException("javac.exe could not start!");
                bool graceful = javacProcess.WaitForExit((int) properties.Timeout);

                if (graceful) {
                    string error = await javacProcess.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(error)) throw new CompileException(error);
                    string output = await javacProcess.StandardOutput.ReadToEndAsync();
                    return output;
                }
                throw new CompileException("The compilation took longer than expected!");
            }
        }

        /// <summary>
        ///     Search for the JDK Directory in the specified path
        /// </summary>
        /// <param name="javaPath">Path to the Java dir (C:\Program Files\Java)</param>
        /// <returns>The found jdk path or null if not found</returns>
        public static string SearchJavaPath(string javaPath) {
            DirectoryInfo javaInfo = new DirectoryInfo(javaPath);
            if (javaInfo.Exists)
                foreach (DirectoryInfo info in javaInfo.EnumerateDirectories())
                    if (info.Name.ToLower().Contains("jdk")) {
                        string javaBinJavac = Path.Combine(info.FullName, "bin", "javac.exe");
                        FileInfo javac = new FileInfo(javaBinJavac);
                        string javaBinJava = Path.Combine(info.FullName, "bin", "java.exe");
                        FileInfo java = new FileInfo(javaBinJava);
                        if (javac.Exists && java.Exists)
                            return info.FullName;
                    }

            return null;
        }
    }
}