using System;

namespace Fiddle.Compilers.Java {
    public class JavaExecuteResult : IExecuteResult {
        public JavaExecuteResult(long time, string stdout, object returnVal, ICompileResult cResult,
            Exception exception) {
            Time = time;
            ConsoleOutput = stdout;
            ReturnValue = returnVal;
            CompileResult = cResult;
            Exception = exception;
            Success = exception == null;
        }

        public long Time { get; set; }

        public bool Success { get; set; }

        public object ReturnValue { get; set; }

        public string ConsoleOutput { get; set; }

        public ICompileResult CompileResult { get; set; }

        public Exception Exception { get; set; }
    }
}