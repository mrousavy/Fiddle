using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.Python
{
    public class PyCompileResult : ICompileResult
    {
        public long Time { get; }
        public bool Success { get; }
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings { get; }
        public IEnumerable<Exception> Errors { get; }


        public PyCompileResult(long time, string code,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors)
        {
            Time = time;
            SourceCode = code;
            Diagnostics = diagnostics;
            Warnings = warnings;
            Errors = errors;
            if (Errors != null)
                Success = !Errors.Any();
        }
    }
}
