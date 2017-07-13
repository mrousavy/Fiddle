using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        public Script<object> Script { get; }


        public CSharpCompileResult(long time, string code, Script<object> script,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors)
        {
            Time = time;
            SourceCode = code;
            Script = script;
            Diagnostics = diagnostics;
            Warnings = warnings;
            Errors = errors;
            if (Errors != null)
                Success = !Errors.Any();
        }

        public async Task<IExecuteResult> Execute()
        {
            Globals globals = new Globals();

            Stopwatch sw = Stopwatch.StartNew();
            ScriptState<object> state = await Script.RunAsync(globals, CatchException);
            sw.Stop();

            object returnValue = state.ReturnValue;
            string stdout = globals.Console.ToString();

            IExecuteResult result = new CSharpExecuteResult(sw.ElapsedMilliseconds, true, stdout, returnValue, this);
            return result;
        }


        public bool CatchException(Exception ex)
        {
            return true;
        }
    }
}
