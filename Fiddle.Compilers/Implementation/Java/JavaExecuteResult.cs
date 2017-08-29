using System;
using System.Diagnostics;

namespace Fiddle.Compilers.Implementation.Java {
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

        public int ExceptionLineNr {
            get {
                if (Exception == null) return -1;
                var trace = new StackTrace(Exception, true);
                if (trace.FrameCount <= 0) return 0;
                var frame = trace.GetFrame(0);
                return frame != default(StackFrame) ? frame.GetFileLineNumber() : 0;
            }
        }
    }
}