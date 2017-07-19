using System;

namespace Fiddle.Compilers.Implementation.VB {
    public class VbExecuteResult : IExecuteResult {
        public VbExecuteResult(long time, string stdout, object returnVal, ICompileResult cResult,
            Exception exception) {
            Time = time;
            ConsoleOutput = stdout;
            ReturnValue = returnVal;
            CompileResult = cResult;
            Exception = exception;
            Success = exception == null;
        }

        public long Time { get; }
        public bool Success { get; }
        public string ConsoleOutput { get; }
        public object ReturnValue { get; }
        public ICompileResult CompileResult { get; }
        public Exception Exception { get; }
    }
}