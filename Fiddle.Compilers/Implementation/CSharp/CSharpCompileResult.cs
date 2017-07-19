using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;

namespace Fiddle.Compilers.Implementation.CSharp {
    public class CSharpCompileResult : ICompileResult {
        public CSharpCompileResult(long time, string code, Script<object> script,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors) {
            Time = time;
            SourceCode = code;
            Script = script;
            Diagnostics = diagnostics ?? new List<IDiagnostic>();
            Warnings = warnings ?? new List<IDiagnostic>();
            Errors = errors;
            if (Errors != null)
                Success = !Errors.Any();
        }

        public Script<object> Script { get; }
        public long Time { get; }
        public bool Success { get; } = true;
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings { get; }
        public IEnumerable<Exception> Errors { get; }
    }
}