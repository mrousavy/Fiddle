using System;
using System.Diagnostics;
using System.IO;

namespace Fiddle.Compilers.Implementation.Java {
    public class JreHelper {
        /// <summary>
        ///     Load java.exe and run execution from arguments
        /// </summary>
        /// <param name="javacPathName">Path to java.exe (Program Files\java\jdk\bin\java.exe)</param>
        /// <param name="className">The class name</param>
        /// <param name="properties">Compilation properties</param>
        /// <param name="commandLineOptions">Any compiler options</param>
        public static string ExecuteJava(string javacPathName, string className,
            IExecutionProperties properties, string commandLineOptions = "") {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = javacPathName,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = commandLineOptions + " " + className,
                WorkingDirectory = Path.GetTempPath()
            };
            using (Process javaProcess = Process.Start(startInfo)) {
                bool graceful = javaProcess != null && javaProcess.WaitForExit((int) properties.Timeout);

                if (graceful) {
                    string error =  javaProcess.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(error)) throw new Exception(error);
                    string output = javaProcess.StandardOutput.ReadToEnd();
                    return output;
                }
                throw new Exception("The execution took longer than expected!");
            }
        }
    }
}