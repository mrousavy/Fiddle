using System;

namespace Fiddle.Compilers.Implementation.Python
{
    public class PyExecuteResult : IExecuteResult
    {
        public long Time { get; }
        public bool Success { get; }
        public object ReturnValue { get; }
        public string ConsoleOutput { get; }
        public ICompileResult CompileResult { get; }
        public Exception Exception { get; }

        public PyExecuteResult(long time, string stdout, object returnVal, ICompileResult cResult, Exception exception)
        {
            Time = time;
            ConsoleOutput = stdout;
            ReturnValue = returnVal;
            CompileResult = cResult;
            Exception = exception;
            Success = exception == null;
        }
    }
}
