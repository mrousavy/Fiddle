using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.LUA {
    public class LuaCompileResult : ICompileResult {
        public LuaCompileResult(long time, string code,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors) {
            Time = time;
            SourceCode = code;
            Diagnostics = diagnostics ?? new List<IDiagnostic>();
            Warnings = warnings ?? new List<IDiagnostic>();
            Errors = errors;
            if (Errors != null)
                Success = !Errors.Any();
        }

        public long Time { get; }
        public bool Success { get; } = true;
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings { get; }
        public IEnumerable<Exception> Errors { get; }
    }
}