using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.VB
{
    public class VbCompileResult : ICompileResult
    {
        public long Time { get; }
        public bool Success { get; }
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings { get; }
        public IEnumerable<Exception> Errors { get; }

        public Script<object> Script { get; }


        public VbCompileResult(long time, string code, Script<object> script,
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
    }
}
