using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Fiddle.Compilers.Implementation.CSharp
{
    public class CSharpCompileResult : ICompileResult
    {
        public long Time { get; }
        public bool Success { get; }
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings { get; }
        public IEnumerable<Exception> Errors { get; }
        public Assembly Assembly { get; }


        public CSharpCompileResult(long time, string code, string[] diagnostics, Assembly assembly)
        {
            Time = time;
            SourceCode = code;
            Assembly = assembly;
            InitDiagnostics(diagnostics);
        }

        private void InitDiagnostics(string[] diagnostics)
        {

        }

        public IExecuteResult Execute()
        {
            Stopwatch sw = Stopwatch.StartNew();
            object returnValue = Assembly.EntryPoint.Invoke(null, null);
            sw.Stop();
            return new CSharpExecuteResult(sw.ElapsedMilliseconds, true, returnValue, returnValue, this);
        }
    }
}
