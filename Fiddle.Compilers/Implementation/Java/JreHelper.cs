using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation.Java {
    public class JreHelper {
        /// <summary>
        /// Load java.exe and run execution from arguments
        /// </summary>
        /// <param name="javacPathName">Path to java.exe (Program Files\java\jdk\bin\java.exe)</param>
        /// <param name="className">The class name</param>
        /// <param name="properties">Compilation properties</param>
        /// <param name="commandLineOptions">Any compiler options</param>
        public static async Task<string> ExecuteJava(string javacPathName, string className, IExecutionProperties properties, string commandLineOptions = "") {
            ProcessStartInfo startInfo = new ProcessStartInfo() {
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
                bool graceful = javaProcess.WaitForExit((int)properties.Timeout);

                if (graceful) {
                    string error = await javaProcess.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(error)) {
                        throw new Exception(error);
                    }
                    string output = await javaProcess.StandardOutput.ReadToEndAsync();
                    return output;
                } else {
                    throw new Exception("The execution took longer than expected!");
                }
            }
        }
    }
}
