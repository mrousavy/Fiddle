namespace Fiddle.Compilers.Implementation.CSharp
{
    public class CSharpExecuteResult : IExecuteResult
    {
        public long Time { get; }
        public bool Success { get; }
        public string ConsoleOutput { get; }
        public object ReturnValue { get; }
        public ICompileResult CompileResult { get; }

        public CSharpExecuteResult(long time, bool success, string stdout, object returnVal, ICompileResult cResult)
        {
            Time = time;
            Success = success;
            ConsoleOutput = stdout;
            ReturnValue = returnVal;
            CompileResult = cResult;
        }
    }
}
